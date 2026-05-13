using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class CategoriaProducto
{
    public int IdCategoriaProducto { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
