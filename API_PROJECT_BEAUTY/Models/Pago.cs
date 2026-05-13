using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int IdCita { get; set; }

    public int IdMetodoPago { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime? FechaPago { get; set; }

    public int IdEstado { get; set; }

    public virtual Citum IdCitaNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual MetodoPago IdMetodoPagoNavigation { get; set; } = null!;
}
