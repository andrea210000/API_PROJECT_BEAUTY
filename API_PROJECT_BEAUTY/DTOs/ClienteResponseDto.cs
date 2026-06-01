namespace API_PROJECT_BEAUTY.DTOs
{
    public class ClienteResponseDto
    {
        //DATOS DE LA TABLA
       
        public int IdCliente { get; set; }
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public int idTipoIdentificacion { get; set; }
        public string numeroIdentificacion { get; set; } = null!;

        //DATOS DE LA TABLA USUARIOS
        public int IdUsuario { get; set; }
        public DateTime fechaRegistro { get; set; }
        public int Estado { get; set; }
        public int IdRol { get; set; } = 4;

    }
}
