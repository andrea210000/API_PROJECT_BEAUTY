using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class CategoriaServicio
{
    public int IdCategoriaServicio { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
