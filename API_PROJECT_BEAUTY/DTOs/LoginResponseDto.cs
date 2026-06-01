namespace API_PROJECT_BEAUTY.DTOs
{
    public class LoginResponseDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = null!;
        public string NombreRol { get; set; } = null!;
    }
}
