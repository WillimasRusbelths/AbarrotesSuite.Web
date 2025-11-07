using System;
using System.Collections.Generic;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class Producto
{
    public long Id { get; set; }

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public decimal? Costo { get; set; }

    public int UnidadId { get; set; }

    public int? CategoriaId { get; set; }

    public decimal Stock { get; set; }

    public bool EsPerecible { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<MovimientosStock> MovimientosStocks { get; set; } = new List<MovimientosStock>();

    public virtual Unidade Unidad { get; set; } = null!;

    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}
