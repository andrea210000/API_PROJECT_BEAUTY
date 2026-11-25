using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using API_PROJECT_BEAUTY.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;


namespace API_PROJECT_BEAUTY.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectBeautyContext _context;

        public AuthController(IConfiguration configuration, ProjectBeautyContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();// se deja como global para que abarque todo el proceso de registro, tanto el registro del cliente como el registro del usuario, esto con el fin de garantizar la integridad de los datos
            try
            {
                //1. REGISTRAR EL CLIENTE
                Cliente nuevoCliente = new Cliente //Creamos un nuevo cliente con la información recibida en el request
                {
                    Nombres = request.Nombres,
                    Apellidos = request.Apellidos,
                    Correo = request.Correo,
                    Telefono = request.Telefono,
                    IdTipoIdentificacion = request.IdTipoIdentificacion,
                    NumeroIdentificacion = request.NumeroIdentificacion
                };

                await _context.Clientes.AddAsync(nuevoCliente); //Agregamos el nuevo cliente a la base de datos
                await _context.SaveChangesAsync();


                int idClienteRegistrado = nuevoCliente.IdCliente;


                //2. REGISTRAR EL USUARIO ASOCIADO AL CLIENTE
                // Encriptar la contraseña antes de guardarla en la base de datos
                string contrasenaEncriptada = CryptoService.Encrypt(request.Contrasena);

                // Creamos un nuevo usuario con la información recibida en el request, y asociamos el id del cliente registrado anteriormente
                Usuario nuevoUsuario = new Usuario
                {

                    Nombre = request.Nombres,
                    Contrasena = contrasenaEncriptada,
                    IdRol = request.IdRol,
                    IdCliente = idClienteRegistrado,// Asociamos el usuario con el cliente registrado
                    IdEstado = (int)request.IdEstado, // Activo por predeterminado
                    FechaRegistro = (DateTime)request.FechaRegistro
                };

                await _context.Usuarios.AddAsync(nuevoUsuario); //Agregamos el nuevo usuario a la base de datos
                await _context.SaveChangesAsync();


                await transaction.CommitAsync();// Confirmamos la transacción, guardando los cambios de manera definitiva en la base de datos

                //3. SE NOTIFICA QUE EL USUARIO Y EL CLIENTE FUERON REGISTRADO EXITOSAMENTE
                return StatusCode(201, new { mensaje = "El cliente y el usuario se registraron existosamente." });
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync(); //EN CASO DE FALLE ALGO, NO GUARDARA NADA Y ENVIARA DE VUELTA LA INFO INGRESADA
                //4. SE NOTIFICA QUE HUBO UN ERROR A LA HORA DE REGISTRAR
                return StatusCode(500, new { mensaje = "ocurrió un error al registrar el usuario o el cliente", error = ex.Message });

            }
        }



       

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); //se deja como global para que abarque todo el proceso de login, tanto la validación del usuario como la generación de los tokens, esto con el fin de garantizar la integridad de los datos y evitar problemas en caso de que algo falle durante el proceso de login.

            try
            {
                //1. VALIDAR DATOS DE ENTRADA CONTRA BD
                string contrasenaEncriptada = CryptoService.Encrypt(request.Contrasena); //Encriptamos la contraseña recibida en el request, para compararla con la contraseña encriptada en la base de datos.
                Usuario usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(x => x.Nombre == request.Usuario && x.Contrasena == contrasenaEncriptada && x.IdEstado == 1);// Validamos que el usuario exista, que la contraseña sea correcta y que el usuario esté activo (IdEstado == 1)

                //2. SE NOTIFICA QUE EL USUARIO O LA CONTRASEÑA SON INCORRECTOS, O QUE EL USUARIO ESTA INACTIVO
                if (usuarioExistente == null) return Unauthorized("Usuario o contraseña erroneos, o usuario inactivo");

                //3. CREAMOS LAS RECLAMACIONES (Claims) PARA EL TOKEN, SON LOS DATOS PUBLICOS QUE VIAJARAN DENTRO DEL TOKEN
                //Aqui estan los datos que viajaran a traves de el token encriptadamente
                var claims = new []
                {
                    new Claim(ClaimTypes.NameIdentifier,usuarioExistente.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuarioExistente.Nombre),
                    new Claim(ClaimTypes.Role, usuarioExistente.IdRol.ToString()),
                    new Claim("Estado", usuarioExistente.IdEstado.ToString()),
                    new Claim("FechaRegistro", usuarioExistente.FechaRegistro.ToString("yyyy-MM-dd")),
                };

                //4. GENERAR EL ACCESS TOKEN (POCA INFO, POCO TIEMPO DE VIDA PARA VALIDAR AUTENTICACION Y AUTORIZACION DE USUARIO)
                var jwtSettings = _configuration.GetSection("Jwt"); 
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                //5. CONFIGURAR EL ACCESS TOKEN 
                var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Tiempo de vida del token
                signingCredentials: creds
                );

                //6. GENERAR UN REFRESH TOKEN, ESTE TOKEN ES EL QUE BRINDARA UN NUEVO ACCESS TOKEN CUANDO ESTE MISMO EXPIRE, LE GENERARA UNO NUEVO
                
                var randomNumber = new byte[32]; // creamos bloque vacio para que pueda almacenar 32 bytes de info random

                using (var rng =  RandomNumberGenerator.Create()) //iniciamos el generador automatico del siste

                {
                    rng.GetBytes(randomNumber); //se llena el bloque con info aleatoria
                }

                string refreshTokenAleatorio = Convert.ToBase64String(randomNumber);//Se convierte el bloque a una cadena de string

                //6. GUARDAMOS EL REFRESH TOKEN EN UNA COOKIE, ESTO CON EL FIN DE QUE NO SE ENVIE ESTE MISMO EN EL JSON Y QUE SE ALMACENE EN EL LADO DEL CLIENTE (NAVEGADOR)
                //ESTA COOKIE(HTTPONLY) SERA ACCESIBLE SOLO PARA EL SERVIDOR, NO PODRA SER LEIDA POR JAVASCRIPT EN EL NAVEGADOR, ESTO AYUDA A PROTEGER EL TOKEN DE ATAQUES DE XSS(CROSS - SITE SCRIPTING)

                // CONFIGURAMOS EL HTTPONLY DE LA COOKIE PARA GUARDAR EL REFRESH TOKEN 
                var opcionesCookie = new CookieOptions
                {
                   HttpOnly = true,// La cookie solo es accesible para el servidor
                   Expires = DateTime.Now.AddDays(7) // La cookie tendrá una vida de 7 días, igual que el refresh token

                };

                //7. GUARDAMOS EL REFRESH TOKEN EN LA COOKIE 
                Response.Cookies.Append("refreshToken", refreshTokenAleatorio, opcionesCookie);

                //8. CREAMOS UN OBJETO REFRESH TOKEN PARA GUARDAR LOS DATOS Y ASOCIARLO AL USUARIO QUE INICIA SESION 

                UsuarioToken refreshToken = new UsuarioToken
                {
                    Id_Usuario = usuarioExistente.IdUsuario,
                    Token = refreshTokenAleatorio,
                    Fecha_Expiracion = DateTime.Now.AddDays(7), //tiene un tiempo de vida de 7 dias
                    Estado = 1 //activo
                };

                //9. GUARDAMOS EL REFRESH TOKEN EN LA BD

                await _context.Usuario_Token.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // ✅ Confirmamos la transacción para que el refresh token SÍ quede guardado en la BD

                //10. ENVIAMOS EL ACCESS TOKEN AL USUARIO 
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return StatusCode(201, new
                {
                    mensaje = "Se incio sesion de manera exitosa",
                    accessToken = tokenString 

                });


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); //EN CASO DE FALLE ALGO, NO GUARDARA NADA Y ENVIARA DE VUELTA LA INFO INGRESADA
                return StatusCode(500, new { mensaje = "Error al iniciar sesión", error = ex.Message }); //SE NOTIFICA ERROR AL INICIAR SESION


            }
        }


        [HttpPost("Refresh")]

        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            //1. VALIDAR El REFRESH TOKEN DE ENTRADA (BUSCAR)
            UsuarioToken usuarioTokenExistente = await _context.Usuario_Token.FirstOrDefaultAsync(x => x.Token == Request.Cookies["refreshToken"]); //Buscamos el refresh token en la base de datos, utilizando el valor que se encuentra en la cookie enviada por el cliente, si el refresh token no existe, o no coincide con ningun registro en la base de datos, entonces el usuario no esta autorizado para obtener un nuevo access token, y se devuelve un error de autenticación.

            //2. VALIDAR QUE EL REFRESH TOKEN EXISTA, QUE ESTE ACTIVO Y QUE NO HAYA EXPIRADO
            if (usuarioTokenExistente == null || usuarioTokenExistente.Estado != 1 || usuarioTokenExistente.Fecha_Expiracion < DateTime.Now)
            {
                return Unauthorized("Refresh Token inválido o expirado");
            }

            //3. VERIFICAR SI EL USUARIO ASOCIADO AL REFRESH TOKEN EXISTE Y ESTA ACTIVO
            Usuario usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == usuarioTokenExistente.Id_Usuario && x.IdEstado == 1);
            // Si el usuario no existe o no está activo, devolvemos un error de autenticación
            if (usuarioExistente == null)
            {
                return Unauthorized("Usuario no encontrado o inactivo");
            }

            //4. CREAR LAS RECLAMACIONES (Claims) PARA EL NUEVO ACCESS TOKEN
            // Son los datos públicos que viajarán dentro del token.

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioExistente.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuarioExistente.Nombre),
                new Claim(ClaimTypes.Role, usuarioExistente.IdRol.ToString()),
                new Claim("Estado", usuarioExistente.IdEstado.ToString()),
                new Claim("FechaRegistro", usuarioExistente.FechaRegistro.ToString("yyyy-MM-dd"))
            };

            //5. GENERAR UN NUEVO ACCESS TOKEN (CONFIGURACION PREDETERMINADA)
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //6. CONFIGURAR EL NUEVO TOKEN, ESTE TIENE POCO TIEMPO DE VIDA 
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Tiempo de vida del token
                signingCredentials: creds
            );

            //7. RENOVACION DE REFRESH TOKEN, CREAMOS UN NUEVO REFRESH TOKEN PARA EL USUARIO, Y ACTUALIZAMOS EL REGISTRO EN LA BASE DE DATOS (ESTO CON El FIN DE BRINDAR MAYOR SEGUIRIDAD Al USUARIO Y El USO DE LOS TOKENS)
            // INACTIVAMOS EL REFRESH TOKEN ANTERIOR
            usuarioTokenExistente.Estado = 2; // Inactivo

            //8. CREAMOS UN NUEVO REFRESH TOKEN PARA EL USUARIO

            var randomNumber = new byte[32]; //Creamos bloque vacio para almacenar 32 bytes de información aleatoria

            using (var rng = RandomNumberGenerator.Create()) //Iniciamos el generador automatico del sistema.

            {
                rng.GetBytes(randomNumber); //Llenamos el bloque vacio con información aleatoria
            }

            string refreshTokenAleatorio = Convert.ToBase64String(randomNumber); //Convertimos el bloque de bytes a una cadena de texto legible

            //9. ACTUALIZAMOS EL REGISTRO DEL REFRESH TOKEN EN LA BASE DE DATOS, ASOCIADO AL USUARIO QUE INICIÓ SESIÓN
            
            UsuarioToken refreshToken = new UsuarioToken
            {
                Id_Usuario = usuarioExistente.IdUsuario,
                Token = refreshTokenAleatorio,
                Fecha_Expiracion = DateTime.Now.AddDays(7), // El refresh token tendrá una vida de 7 días
                Estado = 1 // Activo
            };

            //10. GUARDAR EL NUEVO REFRESH TOKEN EN LA BASE DE DATOS
            await _context.Usuario_Token.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            //11. CONVERTIR EL TOKEN A STRING PARA ENVIARLO AL CLIENTE
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //12.CONFIGURAMOS LA COOKIE PARA GUARDAR EL NUEVO REFRESH TOKEN
            var opcionesCookie = new CookieOptions
            {
                HttpOnly = true,// La cookie solo es accesible para el servidor
                Expires = DateTime.Now.AddDays(7) // La cookie tendrá una vida de 7 días, igual que el refresh token

            };

            //13. GUARDAMOS EL NUEVO REFRESH TOKEN EN LA COOKIE
            Response.Cookies.Append("refreshToken", refreshTokenAleatorio, opcionesCookie);



            //12. ENVIAR EL NUEVO ACCESS TOKEN AL CLIENTE
            return Ok(new
            {
                accesToken = tokenString,
           
            });

           
        }
    }
}
