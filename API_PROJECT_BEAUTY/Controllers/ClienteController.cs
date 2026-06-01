using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using API_PROJECT_BEAUTY.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<Cliente>> registerCliente(ClienteCreateDto data) 
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

    }
}
