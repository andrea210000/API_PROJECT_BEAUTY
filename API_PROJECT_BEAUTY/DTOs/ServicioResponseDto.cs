namespace API_PROJECT_BEAUTY.DTOs
{
    public class ServicioResponseDto
    {
        // Este DTO es el "sobre" que la API le devuelve al frontend.
        // Incluye datos extra (nombre de categoría y estado) que son más
        // amigables de mostrar en las vistas Angular.
        public int IdServicios { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Costo { get; set; }
        public decimal Insumos { get; set; }
        public int DuracionMinutos { get; set; }
        public int IdEstado { get; set; }
        public string EstadoNombre { get; set; } = null!;
        public int IdCategoriaServicio { get; set; }
        public string CategoriaNombre { get; set; } = null!;
    }
}