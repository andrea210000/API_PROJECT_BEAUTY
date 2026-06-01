using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_PROJECT_BEAUTY.DTOs;

    public class RegisterRequestDto
    {
    // Datos del Cliente
    [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre no puede estar vacío.")]
    public required string Nombres { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "El apellido no puede estar vacío.")]
    public required string Apellidos { get; set; }

    /// <example>usuario@correo.com</example>
    [Required(AllowEmptyStrings = false, ErrorMessage = "El correo es obligatorio.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El formato del correo no es válido.")]
    public required string Correo { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "El teléfono no puede estar vacío.")]
    public required string Telefono { get; set; }

    public required int IdTipoIdentificacion { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "El número de identificación es obligatorio.")]
    public required string NumeroIdentificacion { get; set; }

    // Datos del Usuario
    [Required(AllowEmptyStrings = false, ErrorMessage = "La contraseña no puede estar vacía.")]
    public required string Contrasena { get; set; }

    public required int IdRol { get; set; }

    // Campos de auditoría / Solo lectura (Opcionales)
    [JsonIgnore]
    public int? IdCliente { get; set; }
    [JsonIgnore]
    public int? IdUsuario { get; set; }
    [JsonIgnore]
    public int? IdEstado { get; set; } = 1;
    [JsonIgnore]
    public DateTime? FechaRegistro { get; set; } = DateTime.Now;
}

