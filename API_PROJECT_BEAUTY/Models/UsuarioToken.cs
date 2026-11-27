using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PROJECT_BEAUTY.Models
{
    [Table("usuario_token")] // Asegúrate de que coincida con el nombre exacto de la tabla en MySQL
    public class UsuarioToken
    {
        [Key]
        [Column("ID_TOKEN_USUARIO")]
        public int IdUsuarioToken { get; set; }

        // 🌟 CRÍTICO: Vincula esta propiedad directamente con la columna de MySQL
        [Column("ID_USUARIO")]
        public int Id_Usuario { get; set; }

        [Column("TOKEN")]
        public string Token { get; set; }

        [Column("FECHA_EXPIRACION")]
        public DateTime Fecha_Expiracion { get; set; }

        [Column("ESTADO")]
        public int Estado { get; set; }

        // 🌟 SI TIENES ESTA RELACIÓN, INDÍCALE LA LLAVE FORÁNEA EXPLÍCITA:
        [ForeignKey("Id_Usuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}