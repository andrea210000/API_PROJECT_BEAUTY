using API_PROJECT_BEAUTY.Models;

namespace API_PROJECT_BEAUTY.DTOs
{
    public class ProductoUpdateDto
    {
        public string? Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? Stock {  get; set; }
        public string? Medida { get; set; }
        public int? IdCategoriaProducto { get; set; }
        public EstadoProducto? Estado {  get; set; }



    }
}
