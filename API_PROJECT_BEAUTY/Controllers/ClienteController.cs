using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using API_PROJECT_BEAUTY.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_PROJECT_BEAUTY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ClienteController : Controller
    {
        private readonly ProjectBeautyContext _context;

        public ClienteController(ProjectBeautyContext context)
        {
            _context = context;
        }

        [HttpGet ("ListClients")]
        public async Task<ActionResult<List<ClienteResponseDto>>> ListClients()
        {
            try
            {
                // 1.CONSULTAMOS USUARIOS Y AGREGAMOS INFO DE CLIENTES
                var listaUsuarios = await _context.Usuarios
                    .Include(u => u.IdClienteNavigation)
                    .AsNoTracking()
                    .ToListAsync();

                // 2. MAPEAMOS DIRECTAMENTE AL CLIENTERESPONSEDTO
                List<ClienteResponseDto> listaFinalResponse = listaUsuarios
                    .Where(u => u.IdClienteNavigation != null) // Seguridad por si algún usuario no tiene cliente
                    .Where(u => u.IdRol == 4)
                    .Select(u => new ClienteResponseDto
                    {
                        // DATOS DE LA TABLA CLIENTE (u.IdClienteNavigation accede al cliente de ese usuario)
                        IdCliente = u.IdClienteNavigation.IdCliente,
                        Nombres = u.IdClienteNavigation.Nombres,
                        Apellidos = u.IdClienteNavigation.Apellidos,
                        Correo = u.IdClienteNavigation.Correo,
                        Telefono = u.IdClienteNavigation.Telefono,
                        idTipoIdentificacion = u.IdClienteNavigation.IdTipoIdentificacion,
                        numeroIdentificacion = u.IdClienteNavigation.NumeroIdentificacion,

                        // DATOS DE LA TABLA USUARIOS
                        IdUsuario = u.IdUsuario,
                        fechaRegistro = u.FechaRegistro,
                        Estado = u.IdEstado,
                        IdRol = u.IdRol
                    }).ToList();

                // 3. RETORNAMOS LISTA
                return Ok(listaFinalResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al consultar la lista unificada, Error: {ex.Message}");
            }
        }

        [HttpPut("UpdateEstadoUsuario")]
        public async Task<ActionResult<Cliente>> UpdateEstadoUsuario(int idCliente,int estado)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //Validacion de que no sea 0
                if(idCliente == 0 & estado == 0) return BadRequest("El idCliente y estado no puede ser 0");

                //Traemos el usuario
                Usuario? usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.IdCliente == idCliente);

                //Validacion de que el usuario no sea null
                if (usuario != null)
                {
                    //Actualizamos el estado del usuario a
                    usuario.IdEstado = estado;
                    _context.Usuarios.Update(usuario);
                    _context.SaveChanges();

                    await transaction.CommitAsync();
                }
                else
                {
                    return NotFound($"No se encontró un usuario con el id {idCliente}");
                }

                return Ok($"Estado usuario Actualizado id:{idCliente}");
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al inactivar registro de cliente y usuario: {ex.Message}");
            }
            
        }


        [HttpPost ( "registerCliente")]
        public async Task<ActionResult<Cliente>> registerCliente([FromBody] ClienteCreateDto data) 
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cliente = new Cliente
                {
                    Nombres = data.Nombres,
                    Apellidos = data.Apellidos,
                    Correo = data.Correo,
                    Telefono = data.Telefono,
                    IdTipoIdentificacion = data.IdTipoIdentificacion,
                    NumeroIdentificacion = data.NumeroIdentificacion
                };

                // 1. Agregamos el objeto cliente al seguimiento de Entity Framework
                _context.Clientes.Add(cliente);

                await _context.SaveChangesAsync();

                var Usuario = new Usuario
                {
                    IdCliente = cliente.IdCliente,
                    Nombre = cliente.Nombres,
                    Contrasena = CryptoService.Encrypt(data.Contrasena),
                    IdEstado = 1,
                    FechaRegistro = DateTime.Now,
                    IdRol = 4
                };

                _context.Usuarios.Add(Usuario);

                // 2. Guardamos los cambios físicamente en la base de datos MySQL
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                

                var response = new ClienteResponseDto

                {

                    IdCliente = cliente.IdCliente,
                    Nombres = cliente.Nombres,
                    Correo = cliente.Correo,

                };


                // 3. Respondemos con un código 201 (Created) y el cliente guardado
                return StatusCode(201, new {mensaje = "Cliente creado exitosamente"});

            }
            catch (Exception ex) 
            {

                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al crear registro de cliente y usuario: {ex.Message}");
            
            }            
        }
        [HttpGet("newClienteToday")]
        public async Task<ActionResult<int>> newClienteToday()
        {
            var hoy = DateTime.Today;
            var conteo = await _context.Usuarios
                .CountAsync(u => u.FechaRegistro.Date == hoy && u.IdRol == 4);

            return Ok(conteo);
        }

        // ============================================================
        // MI CUENTA (perfil propio, siempre resuelto desde el JWT)
        // ============================================================

        // Auxiliar: identifica al usuario LOGUEADO a partir del token.
        // El id NUNCA llega del formulario, así nadie puede tocar cuentas ajenas.
        private int? ObtenerIdUsuarioLogueado()
        {
            string? claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int idUsuario))
                return null;

            return idUsuario;
        }

        // GET api/Cliente/MiPerfil
        [Authorize]
        [HttpGet("MiPerfil")]
        public async Task<ActionResult<MiPerfilResponseDto>> MiPerfil()
        {
            int? idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario");

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdClienteNavigation).ThenInclude(c => c!.IdTipoIdentificacionNavigation)
                    .Include(u => u.IdRolNavigation)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });
                if (usuario.IdClienteNavigation == null)
                    return BadRequest(new { mensaje = "El usuario no tiene un cliente asociado" });

                var cliente = usuario.IdClienteNavigation;

                return Ok(new MiPerfilResponseDto
                {
                    IdCliente = cliente.IdCliente,
                    Nombres = cliente.Nombres,
                    Apellidos = cliente.Apellidos,
                    Correo = cliente.Correo,
                    Telefono = cliente.Telefono,
                    IdTipoIdentificacion = cliente.IdTipoIdentificacion,
                    TipoIdentificacionNombre = cliente.IdTipoIdentificacionNavigation?.Nombre ?? "",
                    NumeroIdentificacion = cliente.NumeroIdentificacion,
                    IdUsuario = usuario.IdUsuario,
                    Usuario = usuario.Nombre,
                    FechaRegistro = usuario.FechaRegistro,
                    IdRol = usuario.IdRol,
                    RolNombre = usuario.IdRolNavigation?.Nombre ?? ""
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener tu perfil", error = ex.Message });
            }
        }

        // PUT api/Cliente/ActualizarMiPerfil
        [Authorize]
        [HttpPut("ActualizarMiPerfil")]
        public async Task<ActionResult> ActualizarMiPerfil([FromBody] MiPerfilUpdateDto data)
        {
            int? idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario");

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdClienteNavigation)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario?.IdClienteNavigation == null)
                    return BadRequest(new { mensaje = "El usuario no tiene un cliente asociado" });

                var cliente = usuario.IdClienteNavigation;

                // El correo es único: si lo cambian, validamos que nadie más lo use
                if (!string.IsNullOrWhiteSpace(data.Correo) && data.Correo != cliente.Correo)
                {
                    bool correoExiste = await _context.Clientes
                        .AnyAsync(c => c.Correo == data.Correo && c.IdCliente != cliente.IdCliente);

                    if (correoExiste)
                        return BadRequest(new { mensaje = "Ese correo ya está registrado en otra cuenta" });
                }

                // Solo se actualizan los campos que lleguen con valor
                if (!string.IsNullOrWhiteSpace(data.Nombres)) cliente.Nombres = data.Nombres;
                if (!string.IsNullOrWhiteSpace(data.Apellidos)) cliente.Apellidos = data.Apellidos;
                if (!string.IsNullOrWhiteSpace(data.Correo)) cliente.Correo = data.Correo;
                if (data.Telefono != null) cliente.Telefono = data.Telefono;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Perfil actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar el perfil", error = ex.Message });
            }
        }

        // PUT api/Cliente/CambiarPassword
        [Authorize]
        [HttpPut("CambiarPassword")]
        public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordDto data)
        {
            int? idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario");

            if (string.IsNullOrWhiteSpace(data.ContrasenaActual) || string.IsNullOrWhiteSpace(data.ContrasenaNueva))
                return BadRequest(new { mensaje = "Debes escribir la contraseña actual y la nueva" });

            if (data.ContrasenaNueva.Length < 6)
                return BadRequest(new { mensaje = "La nueva contraseña debe tener al menos 6 caracteres" });

            try
            {
                var usuario = await _context.Usuarios.FindAsync(idUsuario);
                if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

                // Comparamos la contraseña actual con la guardada (misma encriptación del login)
                string actualEncriptada = CryptoService.Encrypt(data.ContrasenaActual);
                if (usuario.Contrasena != actualEncriptada)
                    return BadRequest(new { mensaje = "La contraseña actual es incorrecta" });

                usuario.Contrasena = CryptoService.Encrypt(data.ContrasenaNueva);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al cambiar la contraseña", error = ex.Message });
            }
        }

    }
}
