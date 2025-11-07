using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Pago
{
    public long Id { get; set; }

    public long VentaId { get; set; }

    public string Metodo { get; set; } = null!;

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Venta Venta { get; set; } = null!;
}
