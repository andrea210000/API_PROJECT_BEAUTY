namespace API_PROJECT_BEAUTY.DTOs
{
    public class ServicioCreateDto
    {
        // Datos obligatorios que el cliente/admin envían para crear un nuevo servicio.
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Costo { get; set; }
        public decimal Insumos { get; set; }
        public int DuracionMinutos { get; set; }
        public int IdCategoriaServicio { get; set; }

        // Si no se envía, el servicio se crea como ACTIVO (estado 1)
        public int IdEstado { get; set; } = 1;
    }
}