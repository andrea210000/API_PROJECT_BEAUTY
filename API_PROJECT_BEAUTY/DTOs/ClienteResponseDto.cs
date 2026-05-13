namespace API_PROJECT_BEAUTY.DTOs
{
    public class ClienteResponseDto
    {
        public int IdCliente { get; set; }
        public string Nombres { get; set; } = null!;
        public string Correo { get; set; } = null!;
    }
}
