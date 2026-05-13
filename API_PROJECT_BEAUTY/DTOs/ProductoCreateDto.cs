namespace API_PROJECT_BEAUTY.DTOs
{
    public class ProductoCreateDto //Aqui se define el DTO para crear un nuevo producto. Este DTO contiene las propiedades necesarias para crear un producto, como el nombre, la descripción, el precio y la cantidad en stock. Estas propiedades se utilizan para transferir los datos desde el cliente al servidor cuando se realiza una solicitud de creación de producto.
    {
    public int IdCategoriaProducto { get; set; }
        public string Nombre { get; set; } = null!; // Esa declaración de null! indica que la propiedad Nombre no puede ser nula.
    public string Descripcion { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Medida { get; set; } = null!;

    }
}
