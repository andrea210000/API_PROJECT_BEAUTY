namespace API_PROJECT_BEAUTY.DTOs
{
    // Datos de MI propia cuenta, resueltos desde el JWT en el servidor
    public class MiPerfilResponseDto
    {
        // DATOS DE LA TABLA CLIENTE
        public int IdCliente { get; set; }
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string? Telefono { get; set; }
        public int IdTipoIdentificacion { get; set; }
        public string TipoIdentificacionNombre { get; set; } = null!;
        public string NumeroIdentificacion { get; set; } = null!;

        // DATOS DE LA TABLA USUARIO
        public int IdUsuario { get; set; }
        public string Usuario { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public int IdRol { get; set; }
        public string RolNombre { get; set; } = null!;
    }
}