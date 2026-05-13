using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int? IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public int IdEstado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int IdRol { get; set; }

    public virtual ICollection<Citum>? Cita { get; set; } = new List<Citum>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Estado? IdEstadoNavigation { get; set; } = null!;

    public virtual Rol? IdRolNavigation { get; set; } = null!;
}
