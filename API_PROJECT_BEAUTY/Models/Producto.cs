using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int IdCategoriaProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public string Medida { get; set; } = null!;

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual CategoriaProducto IdCategoriaProductoNavigation { get; set; } = null!;
}
