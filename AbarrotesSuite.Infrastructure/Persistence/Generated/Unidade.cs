using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Unidade
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
