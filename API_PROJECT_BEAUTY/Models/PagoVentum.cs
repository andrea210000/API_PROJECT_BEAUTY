using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class PagoVentum
{
    public int IdPago { get; set; }

    public int IdVenta { get; set; }

    public decimal Monto { get; set; }

    public int IdMetodoPago { get; set; }

    public DateTime FechaPago { get; set; }

    public virtual MetodoPago IdMetodoPagoNavigation { get; set; } = null!;

    public virtual Ventum IdVentaNavigation { get; set; } = null!;
}
