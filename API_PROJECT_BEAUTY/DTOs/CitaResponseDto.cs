namespace API_PROJECT_BEAUTY.DTOs;

public class CitaServicioResponseDto
{
    public int IdServicio { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal? PrecioAplicado { get; set; }
}

public class CitaResponseDto
{
    public int IdCita { get; set; }
    public DateTime FechaHora { get; set; }
    public int IdEstado { get; set; }
    public string EstadoNombre { get; set; } = null!;
    public string? Notas { get; set; }
    public decimal Total { get; set; }
    public List<CitaServicioResponseDto> Servicios { get; set; } = new();
}