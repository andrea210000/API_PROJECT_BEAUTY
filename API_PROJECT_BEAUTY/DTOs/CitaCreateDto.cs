using System.ComponentModel.DataAnnotations;

namespace API_PROJECT_BEAUTY.DTOs;

public class CitaCreateDto
{
    // Fecha y hora local de la cita (ej: 2026-09-25T15:30:00)
    [Required(ErrorMessage = "Debe indicar la fecha y hora de la cita")]
    public DateTime FechaHora { get; set; }

    // Lista con los IDs de los servicios que pide el cliente (puede pedir varios)
    [Required, MinLength(1, ErrorMessage = "Debe seleccionar al menos un servicio")]
    public List<int> IdServicios { get; set; } = new();

    // Notas adicionales opcionales (alergias, preferencias, etc.)
    [StringLength(255)]
    public string? Notas { get; set; }
}