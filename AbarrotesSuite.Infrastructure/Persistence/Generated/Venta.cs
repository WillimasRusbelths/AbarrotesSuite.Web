using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Venta
{
    public long Id { get; set; }

    public DateTime Fecha { get; set; }

    public long UsuarioId { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}
