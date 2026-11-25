using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_PROJECT_BEAUTY.Controllers
{
    [Route("api/[controller]")] // Ruta base: api/Servicio
    [ApiController]
    public class ServicioController : ControllerBase
    {
        private readonly ProjectBeautyContext _context;

        public ServicioController(ProjectBeautyContext context)
        {
            _context = context;
        }

        // GET api/Servicio/ObtenerServicios?soloActivos=true
        [HttpGet("ObtenerServicios")]
        public async Task<ActionResult<List<ServicioResponseDto>>> ObtenerServicios([FromQuery] bool soloActivos = false)
        {
            try
            {
                var query = _context.Servicios
                    .Include(s => s.IdCategoriaServicioNavigation) // trae el nombre de la categoría
                    .Include(s => s.IdEstadoNavigation)            // trae el nombre del estado
                    .AsQueryable();

                if (soloActivos)
                    query = query.Where(s => s.IdEstado == 1);

                var lista = await query
                    .Select(s => new ServicioResponseDto
                    {
                        IdServicios = s.IdServicios,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        Costo = s.Costo,
                        Insumos = s.Insumos,
                        DuracionMinutos = s.DuracionMinutos,
                        IdEstado = s.IdEstado,
                        EstadoNombre = s.IdEstadoNavigation.Nombre,
                        IdCategoriaServicio = s.IdCategoriaServicio,
                        CategoriaNombre = s.IdCategoriaServicioNavigation.NombreCategoria
                    })
                    .ToListAsync();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener los servicios", error = ex.Message });
            }
        }

        // GET api/Servicio/ObtenerServicio/1
        [HttpGet("ObtenerServicio/{idServicio:int}")]
        public async Task<ActionResult<ServicioResponseDto>> ObtenerServicio(int idServicio)
        {
            var s = await _context.Servicios
                .Include(x => x.IdCategoriaServicioNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Where(x => x.IdServicios == idServicio)
                .Select(x => new ServicioResponseDto
                {
                    IdServicios = x.IdServicios,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Costo = x.Costo,
                    Insumos = x.Insumos,
                    DuracionMinutos = x.DuracionMinutos,
                    IdEstado = x.IdEstado,
                    EstadoNombre = x.IdEstadoNavigation.Nombre,
                    IdCategoriaServicio = x.IdCategoriaServicio,
                    CategoriaNombre = x.IdCategoriaServicioNavigation.NombreCategoria
                })
                .FirstOrDefaultAsync();

            if (s == null) return NotFound(new { mensaje = "Servicio no encontrado" });
            return Ok(s);
        }

        // POST api/Servicio/InsertarServicio
        [HttpPost("InsertarServicio")]
        public async Task<ActionResult> InsertarServicio([FromBody] ServicioCreateDto data)
        {
            if (string.IsNullOrWhiteSpace(data.Nombre)) return BadRequest("El nombre del servicio es obligatorio.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevo = new Servicio
                {
                    IdCategoriaServicio = data.IdCategoriaServicio,
                    Nombre = data.Nombre,
                    Descripcion = data.Descripcion,
                    Costo = data.Costo,
                    Insumos = data.Insumos,
                    DuracionMinutos = data.DuracionMinutos,
                    IdEstado = data.IdEstado
                };

                _context.Servicios.Add(nuevo);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(201, new { mensaje = "Servicio creado exitosamente", id = nuevo.IdServicios });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al crear el servicio", error = ex.Message });
            }
        }

        // PUT api/Servicio/ActualizarServicio?idServicio=1
        [HttpPut("ActualizarServicio")]
        public async Task<ActionResult> ActualizarServicio(int idServicio, [FromBody] ServicioUpdateDto data)
        {
            try
            {
                var existente = await _context.Servicios.FindAsync(idServicio);
                if (existente == null) return NotFound(new { mensaje = "Servicio no encontrado" });

                // Solo actualiza los campos enviados (patrón ?? )
                existente.Nombre = data.Nombre ?? existente.Nombre;
                existente.Descripcion = data.Descripcion ?? existente.Descripcion;
                existente.Costo = data.Costo ?? existente.Costo;
                existente.Insumos = data.Insumos ?? existente.Insumos;
                existente.DuracionMinutos = data.DuracionMinutos ?? existente.DuracionMinutos;
                existente.IdCategoriaServicio = data.IdCategoriaServicio ?? existente.IdCategoriaServicio;
                existente.IdEstado = data.IdEstado ?? existente.IdEstado;

                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Servicio actualizado", id = existente.IdServicios });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar el servicio", error = ex.Message });
            }
        }

        // PUT api/Servicio/CambiarEstadoServicio?idServicio=1&estado=1
        [HttpPut("CambiarEstadoServicio")]
        public async Task<ActionResult> CambiarEstadoServicio(int idServicio, int estado)
        {
            if (estado < 1 || estado > 2) return BadRequest("El estado debe ser 1 (activo) o 2 (inactivo)");

            var existente = await _context.Servicios.FindAsync(idServicio);
            if (existente == null) return NotFound(new { mensaje = "Servicio no encontrado" });

            existente.IdEstado = estado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Estado actualizado", id = idServicio, estado });
        }
    }
}