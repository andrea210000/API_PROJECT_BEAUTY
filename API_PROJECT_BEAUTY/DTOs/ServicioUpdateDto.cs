namespace API_PROJECT_BEAUTY.DTOs
{
    public class ServicioUpdateDto
    {
        // Todas las propiedades son opcionales (nullable) para poder actualizar
        // solo los campos que el usuario envíe. Si un campo no viene, se conserva el actual.
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Costo { get; set; }
        public decimal? Insumos { get; set; }
        public int? DuracionMinutos { get; set; }
        public int? IdCategoriaServicio { get; set; }
        public int? IdEstado { get; set; }
    }
}