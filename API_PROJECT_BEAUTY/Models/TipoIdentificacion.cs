using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class TipoIdentificacion
{
    public int IdTipoIdentificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
