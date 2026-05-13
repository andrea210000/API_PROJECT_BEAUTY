using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class MetodoPago
{
    public int IdMetodoPago { get; set; }

    public string NombreMetodo { get; set; } = null!;

    public virtual ICollection<PagoVentum> PagoVenta { get; set; } = new List<PagoVentum>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
