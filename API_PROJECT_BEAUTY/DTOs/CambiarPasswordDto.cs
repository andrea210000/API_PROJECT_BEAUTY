namespace API_PROJECT_BEAUTY.DTOs
{
    // Cambio de contraseña: se valida la actual antes de guardar la nueva
    public class CambiarPasswordDto
    {
        public string ContrasenaActual { get; set; } = null!;
        public string ContrasenaNueva { get; set; } = null!;
    }
}