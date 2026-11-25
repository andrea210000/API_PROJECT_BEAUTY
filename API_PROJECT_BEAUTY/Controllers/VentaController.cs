using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace API_PROJECT_BEAUTY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly ProjectBeautyContext _context;

        public VentaController(ProjectBeautyContext context)
        {
            _context = context;
        }

        // Auxiliar: identifica al usuario LOGUEADO a partir del token
        private int? ObtenerIdUsuarioLogueado()
        {
            string? claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int idUsuario))
                return null;

            return idUsuario;
        }

        // GET api/Venta/MetodosPago
        [HttpGet("MetodosPago")]
        public async Task<ActionResult<IEnumerable<MetodoPagoDto>>> MetodosPago()
        {
            var metodos = await _context.MetodoPagos
                .OrderBy(m => m.IdMetodoPago)
                .Select(m => new MetodoPagoDto
                {
                    IdMetodoPago = m.IdMetodoPago,
                    NombreMetodo = m.NombreMetodo
                })
                .ToListAsync();

            return Ok(metodos);
        }

        // POST api/Venta/CrearVenta
        // Recibe el carrito y el método de pago. En UNA transacción:
        // valida stock, crea la venta + detalle, descuenta stock y registra el pago.
        // Si algo falla, no se guarda nada.
        [Authorize]
        [HttpPost("CrearVenta")]
        public async Task<ActionResult> CrearVenta([FromBody] VentaCreateDto data)
        {
            int? idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario");

            if (data.Items == null || data.Items.Count == 0)
                return BadRequest(new { mensaje = "El carrito está vacío" });

            if (data.Items.Any(i => i.IdProducto <= 0 || i.Cantidad <= 0))
                return BadRequest(new { mensaje = "Hay productos o cantidades inválidas en el carrito" });

            var metodo = await _context.MetodoPagos.FindAsync(data.IdMetodoPago);
            if (metodo == null)
                return BadRequest(new { mensaje = "Método de pago no válido" });

            // Tarjeta (crédito o débito): solo validación de formato, NO se guarda nada
            bool esTarjeta = data.IdMetodoPago == 2 || data.IdMetodoPago == 6;
            if (esTarjeta)
            {
                var t = data.Tarjeta;
                string numero = (t?.Numero ?? "").Replace(" ", "");
                bool tarjetaOk = t != null
                    && !string.IsNullOrWhiteSpace(t.Titular)
                    && numero.Length == 16 && numero.All(char.IsDigit)
                    && Regex.IsMatch(t.Vencimiento ?? "", @"^(0[1-9]|1[0-2])\/\d{2}$")
                    && Regex.IsMatch(t.Cvv ?? "", @"^\d{3}$");

                if (!tarjetaOk)
                    return BadRequest(new { mensaje = "Los datos de la tarjeta no son válidos (número de 16 dígitos, MM/AA y CVV de 3)" });
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdClienteNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario?.IdClienteNavigation == null)
                return BadRequest(new { mensaje = "El usuario no tiene un cliente asociado" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Los items repetidos del carrito se agrupan (2 de A + 3 de A = 5 de A)
                var itemsAgrupados = data.Items
                    .GroupBy(i => i.IdProducto)
                    .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(i => i.Cantidad) })
                    .ToList();

                decimal total = 0;
                var detalles = new List<DetalleVentum>();

                foreach (var item in itemsAgrupados)
                {
                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p => p.IdProducto == item.IdProducto);

                    if (producto == null)
                        return BadRequest(new { mensaje = "Un producto del carrito ya no está disponible" });

                    if (producto.Stock < item.Cantidad)
                        return BadRequest(new { mensaje = $"Stock insuficiente de \"{producto.Nombre}\" (quedan {producto.Stock})" });

                    // Descuento de stock
                    producto.Stock -= item.Cantidad;

                    decimal subtotalLinea = producto.Precio * item.Cantidad;
                    total += subtotalLinea;

                    detalles.Add(new DetalleVentum
                    {
                        IdProducto = producto.IdProducto,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotalLinea
                    });
                }

                // Venta
                var venta = new Ventum
                {
                    IdCliente = usuario.IdClienteNavigation.IdCliente,
                    Fecha = DateTime.Now,
                    Total = total
                };
                _context.Venta.Add(venta);
                await _context.SaveChangesAsync(); // para tener el IdVenta

                // Detalle de la venta
                foreach (var d in detalles)
                {
                    d.IdVenta = venta.IdVenta;
                    _context.DetalleVenta.Add(d);
                }

                // Pago de la venta (1 venta = 1 pago)
                _context.PagoVenta.Add(new PagoVentum
                {
                    IdVenta = venta.IdVenta,
                    Monto = total,
                    IdMetodoPago = metodo.IdMetodoPago,
                    FechaPago = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Compra realizada correctamente",
                    idVenta = venta.IdVenta,
                    total
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "No se pudo procesar la compra", error = ex.Message });
            }
        }

        // GET api/Venta/MisVentas
        [Authorize]
        [HttpGet("MisVentas")]
        public async Task<ActionResult<IEnumerable<VentaResponseDto>>> MisVentas()
        {
            int? idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario");

            var usuario = await _context.Usuarios
                .Include(u => u.IdClienteNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario?.IdClienteNavigation == null)
                return BadRequest(new { mensaje = "El usuario no tiene un cliente asociado" });

            int idCliente = usuario.IdClienteNavigation.IdCliente;

            var ventas = await _context.Venta
                .Where(v => v.IdCliente == idCliente)
                .OrderByDescending(v => v.Fecha)
                .Select(v => new VentaResponseDto
                {
                    IdVenta = v.IdVenta,
                    Fecha = v.Fecha,
                    Total = v.Total,
                    MetodoPago = v.PagoVentum != null ? v.PagoVentum.IdMetodoPagoNavigation.NombreMetodo : "",
                    Detalle = v.DetalleVenta
                        .Select(d => new VentaDetalleResponseDto
                        {
                            IdProducto = d.IdProducto,
                            Producto = d.IdProductoNavigation.Nombre,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }
    }
}