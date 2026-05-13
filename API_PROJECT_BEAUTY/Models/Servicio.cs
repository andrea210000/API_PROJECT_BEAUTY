using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Servicio
{
    public int IdServicios { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal Costo { get; set; }

    public decimal Insumos { get; set; }

    public int DuracionMinutos { get; set; }

    public int IdEstado { get; set; }

    public int IdCategoriaServicio { get; set; }

    public virtual ICollection<CitaServicio> CitaServicios { get; set; } = new List<CitaServicio>();

    public virtual CategoriaServicio IdCategoriaServicioNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;
}
