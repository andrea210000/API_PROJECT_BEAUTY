using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_PROJECT_BEAUTY.Controllers
{
    [Route("api/[controller]")] // Ruta base: api/Cita
    [ApiController]
    public class CitaController : ControllerBase
    {
        private readonly ProjectBeautyContext _context;


        public CitaController(ProjectBeautyContext context)
        {
            _context = context;
        }

        // 🔐 Método auxiliar: identifica al usuario LOGUEADO a partir del JWT.
        // No le pedimos al frontend el idCliente: lo resolvemos del lado del servidor
        // (el token lo firmamos nosotros, así que es confiable).
        private async Task<Usuario?> ObtenerUsuarioLogueado()
        {
            string? claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int idUsuario))
                return null;

            return await _context.Usuarios
                .Include(u => u.IdClienteNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        // POST api/Cita/CrearCita  (solo usuarios autenticados)
        [Authorize]
        [HttpPost("CrearCita")]
        public async Task<IActionResult> CrearCita([FromBody] CitaCreateDto data)
        {
            var usuario = await ObtenerUsuarioLogueado();
            if (usuario == null) return Unauthorized("No se pudo identificar al usuario");
            if (usuario.IdClienteNavigation == null)
                return BadRequest("El usuario no tiene un cliente asociado");
            if (data.IdServicios == null || data.IdServicios.Count == 0)
                return BadRequest("Debe seleccionar al menos un servicio");
            if (data.FechaHora < DateTime.Now)
                return BadRequest("La fecha y hora de la cita deben ser futuras");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Traer los servicios seleccionados para validar que existen y capturar su precio actual
                var idsUnicos = data.IdServicios.Distinct().ToList();
                var servicios = await _context.Servicios
                    .Where(s => idsUnicos.Contains(s.IdServicios))
                    .ToListAsync();

                if (servicios.Count != idsUnicos.Count)
                    return BadRequest("Uno o más servicios seleccionados no existen");

                // 2. Crear la CABECERA de la cita (estado 4 = PENDIENTE)
                //    IdUsuario: por ahora quien agenda (posteriormente: estilista asignado)
                var cita = new Citum
                {
                    IdCliente = usuario.IdClienteNavigation.IdCliente,
                    IdUsuario = usuario.IdUsuario,
                    IdEstado = 4,
                    FechaHora = data.FechaHora,
                    Notas = data.Notas
                };
                _context.Cita.Add(cita);
                await _context.SaveChangesAsync(); // necesitamos el IdCita

                // 3. Crear el DETALLE (una fila en cita_servicio por cada servicio)
                //    PrecioAplicado congela el precio del servicio en el momento de agendar
                foreach (var s in servicios)
                {
                    _context.CitaServicios.Add(new CitaServicio
                    {
                        IdCita = cita.IdCita,
                        IdServicio = s.IdServicios,
                        PrecioAplicado = s.Costo
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(201, new { mensaje = "Cita agendada exitosamente", idCita = cita.IdCita });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al agendar la cita", error = ex.Message });
            }
        }

        // GET api/Cita/MisCitas  (citas del usuario logueado)
        [Authorize]
        [HttpGet("MisCitas")]
        public async Task<ActionResult<List<CitaResponseDto>>> MisCitas()
        {
            var usuario = await ObtenerUsuarioLogueado();
            if (usuario == null) return Unauthorized();
            if (usuario.IdCliente == null) return BadRequest("El usuario no tiene un cliente asociado");

            try
            {
                var citas = await _context.Cita
                    .Where(c => c.IdCliente == usuario.IdCliente)
                    .Include(c => c.IdEstadoNavigation)
                    .Include(c => c.CitaServicios).ThenInclude(cs => cs.IdServicioNavigation)
                    .OrderBy(c => c.FechaHora)
                    .ToListAsync();

                var resultado = citas.Select(c => new CitaResponseDto
                {
                    IdCita = c.IdCita,
                    FechaHora = c.FechaHora,
                    IdEstado = c.IdEstado,
                    EstadoNombre = c.IdEstadoNavigation?.Nombre ?? "",
                    Notas = c.Notas,
                    Total = c.CitaServicios.Sum(cs => cs.PrecioAplicado ?? 0),
                    Servicios = c.CitaServicios.Select(cs => new CitaServicioResponseDto
                    {
                        IdServicio = cs.IdServicio,
                        Nombre = cs.IdServicioNavigation?.Nombre ?? "",
                        PrecioAplicado = cs.PrecioAplicado
                    }).ToList()
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener tus citas", error = ex.Message });
            }
        }

        // PUT api/Cita/CancelarCita/1  (el cliente cancela su propia cita)
        [Authorize]
        [HttpPut("CancelarCita/{idCita:int}")]
        public async Task<IActionResult> CancelarCita(int idCita)
        {
            var usuario = await ObtenerUsuarioLogueado();
            if (usuario == null) return Unauthorized();

            var cita = await _context.Cita.FirstOrDefaultAsync(c => c.IdCita == idCita && c.IdCliente == usuario.IdCliente);
            if (cita == null) return NotFound(new { mensaje = "Cita no encontrada" });

            if (cita.IdEstado == 6 || cita.IdEstado == 7) // 6 = REALIZADA, 7 = CANCELADA
                return BadRequest("La cita ya no se puede cancelar");

            cita.IdEstado = 7; // CANCELADA
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cita cancelada" });
        }
    }
}
