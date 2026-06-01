using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_PROJECT_BEAUTY.DTOs;
using API_PROJECT_BEAUTY.Models;
using Microsoft.EntityFrameworkCore;


namespace API_PROJECT_BEAUTY.Controllers
{
    [Route("api/[controller]")] // Define la ruta base para este controlador, por ejemplo: api/producto
    [ApiController]
    public class ProductoController : ControllerBase
    {
       
        private readonly ProjectBeautyContext _context;// Inyección de dependencias para acceder al contexto de la base de datos, es decir, permite la conexión y manipulación de los datos almacenados en la base de datos.

        // Constructor del controlador que recibe el contexto de la base de datos a través de la inyección de dependencias.
        public ProductoController(ProjectBeautyContext context)
        {
            _context = context;// Asigna el contexto de la base de datos a la variable privada _context para su uso en los métodos del controlador.
        }
        [HttpPost (Name = "InsertarProductos")]
        public async Task<ActionResult> InsertarProductos(ProductoCreateDto data)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); // Inicia una transacción de base de datos de forma asíncrona, lo que permite agrupar varias operaciones de base de datos en una sola unidad de trabajo. Si alguna operación falla, se puede revertir toda la transacción para mantener la integridad de los datos.

            if (data == null) return BadRequest("Los datos del producto son obligatorios.");// Verifica si los datos del producto son nulos y devuelve una respuesta de error si es así.

            try //Mapeo: Aqui pasamos los datos de DTO a el modelo real de la base de datos,es decir, se crea una nueva instancia de la clase Producto y se asignan los valores correspondientes desde el DTO.
            {
                var nuevoProducto = new Producto
                {
                    IdCategoriaProducto = data.IdCategoriaProducto,
                    Nombre = data.Nombre,
                    Descripcion = data.Descripcion,
                    Precio = data.Precio,
                    Stock = data.Stock,
                    Medida = data.Medida,

                };

                _context.Productos.Add(nuevoProducto);// Agrega el nuevo producto al contexto de la base de datos, lo que prepara el producto para ser guardado en la base de datos.

                await _context.SaveChangesAsync();// Guarda los cambios en la base de datos de forma asíncrona, lo que persiste el nuevo producto en la base de datos.

                await transaction.CommitAsync();

                // Si todo sale bien, devuelve una respuesta con el código de estado 201 (Creado) y un mensaje indicando que el producto fue creado exitosamente.
                return StatusCode(201, new { mensaje = "¡Producto Creado!", id = nuevoProducto.IdProducto });

            }
            catch (Exception ex) 
            {
            
            //si algo falla, devolveremos un error 500 con el mensaje de error.
                return StatusCode(500, new { mensaje = "Error al crear el producto.", error = ex.Message });

            }
        }
            [HttpGet (Name = "ObtenerProductos")]
            public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> ObtenerProductos() // Define un método asincrónico que devuelve una lista de productos en formato DTO para ser consumidos por el cliente.
            {

                try {

                    // Consultamos la tabla productos
                    var listaProductos = await _context.Productos
                    // Traemos la relacion con la tabla categoria producto para obtener el nombre de la categoria
                    .Include(p => p.IdCategoriaProductoNavigation) //esto trae los datos relacionados de la tabla categoria producto, es decir, el nombre de la categoria a la que pertenece cada producto.
                    .Select(p => new ProductoResponseDto
                    {
                        IdProducto = p.IdProducto,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        Medida = p.Medida,
                        CategoriaNombre = p.IdCategoriaProductoNavigation.NombreCategoria,

                    })
                    .ToListAsync(); // Ejecuta la consulta de forma asíncrona y devuelve una lista de productos en formato DTO.

                    return Ok(listaProductos);
                }
                catch (Exception ex)
                {
                    //si algo falla, devolveremos un error 500 con el mensaje de error.
                    return StatusCode(500, new { mensaje = "Error al obtener los productos.", error = ex.Message });


                }

            }
        [HttpPut (Name = "ActualizarProducto")]

        public async Task<ActionResult> ActualizarProducto (int idProducto, ProductoUpdateDto data) 
        {

            using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Busca el producto por su ID en la base de datos utilizando el método FindAsync. Si el producto no existe, devuelve una respuesta de error 404 (No Encontrado) con un mensaje.
                    var ProductoExistente = await _context.Productos.FindAsync(idProducto);

                    
                        if (ProductoExistente == null)
                        {
                            return NotFound(new { mensaje = "Producto no encontrado." }); // Si el producto no existe, devuelve una respuesta de error 404 (No Encontrado) con un mensaje.
                        }


                // Si el producto existe, actualiza sus propiedades con los valores proporcionados en el DTO. Si alguna propiedad del DTO es nula, se mantiene el valor existente del producto.
                     ProductoExistente.Nombre = data.Nombre ?? ProductoExistente.Nombre;
                     ProductoExistente.Descripcion = data.Descripcion ?? ProductoExistente.Descripcion;
                     ProductoExistente.Precio = data.Precio ?? ProductoExistente.Precio;
                     ProductoExistente.Stock = data.Stock ?? ProductoExistente.Stock;
                     ProductoExistente.Medida = data.Medida ?? ProductoExistente.Medida;
                     ProductoExistente.Estado = data.Estado ?? ProductoExistente.Estado;

                      await _context.SaveChangesAsync(); // Guarda los cambios en la bd de forma asincronica

                      await transaction.CommitAsync();// Si todo sale bien, se confirma la transacción.

                return StatusCode(201, new { mensaje = "¡Producto Actualizado!", id = ProductoExistente.IdProducto }); // Si la actualización es exitosa, devuelve una respuesta con el código de estado 201 (Creado) y un mensaje indicando que el producto fue actualizado exitosamente.

                    
                }

            catch (Exception ex)
            {

                //si algo falla, devolveremos un error 500 con el mensaje de error.
                return StatusCode(500, new { mensaje = "Error al actualizar el producto.", error = ex.Message });
            }


        }

            
    }
}
