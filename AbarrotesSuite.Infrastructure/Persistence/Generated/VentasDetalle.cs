using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class VentasDetalle
{
    public long Id { get; set; }

    public long VentaId { get; set; }

    public long ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal Precio { get; set; }

    public decimal? Importe { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Venta Venta { get; set; } = null!;
}
