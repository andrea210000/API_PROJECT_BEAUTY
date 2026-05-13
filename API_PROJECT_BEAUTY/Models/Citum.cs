using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Citum
{
    public int IdCita { get; set; }

    public int IdCliente { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstado { get; set; }

    public DateTime FechaHora { get; set; }

    public string? Notas { get; set; }

    public virtual ICollection<CitaServicio> CitaServicios { get; set; } = new List<CitaServicio>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
