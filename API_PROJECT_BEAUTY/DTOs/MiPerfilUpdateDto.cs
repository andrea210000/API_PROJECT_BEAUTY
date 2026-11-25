namespace API_PROJECT_BEAUTY.DTOs
{
    // Campos editables del perfil propio.
    // El usuario NO se edita aquí (es único y se usa para iniciar sesión).
    public class MiPerfilUpdateDto
    {
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
    }
}