using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class CitaServicio
{
    public int IdCitaServicio { get; set; }

    public int IdCita { get; set; }

    public int IdServicio { get; set; }

    public decimal? PrecioAplicado { get; set; }

    public virtual Citum IdCitaNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
