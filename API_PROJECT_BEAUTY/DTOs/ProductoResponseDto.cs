using API_PROJECT_BEAUTY.Models;

namespace API_PROJECT_BEAUTY.DTOs
{
    public class ProductoResponseDto
    {
        public string CategoriaNombre { get; set; } = null!;
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Medida { get; set; } = null!;
        public EstadoProducto Estado { get; set; }

    }
}
