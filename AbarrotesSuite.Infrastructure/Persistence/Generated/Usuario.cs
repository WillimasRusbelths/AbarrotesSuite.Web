using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Usuario
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string HashPassword { get; set; } = null!;

    public bool Activo { get; set; }

    public long? EmpleadoId { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual Empleado? Empleado { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();

    public virtual ICollection<Role> Rols { get; set; } = new List<Role>();
}
