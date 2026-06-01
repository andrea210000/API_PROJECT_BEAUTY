using System.ComponentModel.DataAnnotations;

namespace API_PROJECT_BEAUTY.DTOs
{
    public class LoginRequestDto
    {
        [Required (AllowEmptyStrings = false, ErrorMessage = "El nombre es requerido")]
        public string Usuario {get; set; } = null!;
        [Required (AllowEmptyStrings = false, ErrorMessage = "La contraseña es requerida")]
        public string Contrasena { get; set; } = null!;

    }
}
