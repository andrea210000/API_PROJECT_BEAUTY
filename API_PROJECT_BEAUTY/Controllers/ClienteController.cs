using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using API_PROJECT_BEAUTY.Tools;
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

        [HttpGet(Name = "TraerClientes")]
        public async Task<ActionResult<List<ClienteResponseDto>>> TraerClientes()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //Traemos la lista de clientes activos
                List<Cliente> listaClientes = await _context.Clientes.ToListAsync();

                //Mapeamos la lista de clientes a una lista de ClienteResponseDto
                List<ClienteResponseDto> listaClientesResponse = listaClientes.Select(c => new ClienteResponseDto
                {
                    IdCliente = c.IdCliente,
                    Nombres = c.Nombres,
                    Correo = c.Correo
                }).ToList();

                await transaction.CommitAsync();

                return Ok(listaClientesResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al consultar clientes, Error: {ex.Message}");
            }
        }


        [HttpPut(Name = "ActualizarEstadoUsuario")]
        public async Task<ActionResult<Cliente>> ActualizarEstadoUsuario(int idCliente,int estado)
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
                    //Actualizamos el estado del usuario a inactivo (2)
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


        [HttpPost (Name = "InsertarCliente")]
        public async Task<ActionResult<Cliente>> InsertarCliente(ClienteCreateDto data) 
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
                    Nombre = cliente.Correo,
                    Contrasena = PasswordHasher.HashPassword(Random.Shared.Next(1, 101).ToString()),
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
                return StatusCode(201, response);

            }
            catch (Exception ex) 
            {

                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al crear registro de cliente y usuario: {ex.Message}");
            
            }

            
        }
    }
}
