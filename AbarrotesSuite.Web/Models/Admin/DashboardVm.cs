namespace AbarrotesSuite.Web.Models.Admin;

public class DashboardVm
{
    // KPIs
    public int TicketsHoy { get; set; }
    public decimal MontoHoy { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal UnidadesVendidasHoy { get; set; }

    // Caja
    public List<CajaMetodoRow> CajaPorMetodo { get; set; } = new();
    public decimal CajaTotal { get; set; }

    // Listados
    public List<VentaRow> UltimasVentas { get; set; } = new();
    public List<ProductoRow> TopProductosUnidades { get; set; } = new();
    public List<ProductoRow> TopProductosMonto { get; set; } = new();
    public List<ProductoRow> BajoStock { get; set; } = new();
    public List<UsuarioRow> UsuariosRecientes { get; set; } = new();
    public int AnuladasHoy { get; set; }

    public class CajaMetodoRow { public string Metodo { get; set; } = string.Empty; public decimal Monto { get; set; } }
    public class VentaRow { public long Id { get; set; } public DateTime Fecha { get; set; } public decimal Total { get; set; } public string Estado { get; set; } = string.Empty; public string Usuario { get; set; } = string.Empty; }
    public class ProductoRow { public long Id { get; set; } public string? Sku { get; set; } public string Nombre { get; set; } = string.Empty; public decimal Stock { get; set; } public decimal? Unidades { get; set; } public decimal? Monto { get; set; } }
    public class UsuarioRow { public long Id { get; set; } public string Username { get; set; } = string.Empty; public DateTime CreadoEn { get; set; } public bool Activo { get; set; } }
}
