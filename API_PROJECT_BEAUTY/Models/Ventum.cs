using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public int IdCliente { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual PagoVentum? PagoVentum { get; set; }
}
