using System;
using System.Collections.Generic;

namespace API_PROJECT_BEAUTY.Models;

public partial class Cliente
{
    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public int IdCliente { get; set; }

    public int IdTipoIdentificacion { get; set; }

    public string NumeroIdentificacion { get; set; }

    public virtual ICollection<Citum>? Cita { get; set; } = new List<Citum>();

    public virtual TipoIdentificacion? IdTipoIdentificacionNavigation { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }

    public virtual ICollection<Ventum>? Venta { get; set; } = new List<Ventum>();
}
