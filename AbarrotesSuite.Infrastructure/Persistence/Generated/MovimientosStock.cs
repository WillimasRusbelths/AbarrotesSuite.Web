using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class MovimientosStock
{
    public long Id { get; set; }

    public long ProductoId { get; set; }

    public string Tipo { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public string? Referencia { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
