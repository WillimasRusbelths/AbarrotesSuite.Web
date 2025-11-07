namespace AbarrotesSuite.Web.Models.Cajero;

public class CashierDashboardVm
{
    public int TicketsHoy { get; set; }
    public decimal MontoHoy { get; set; }
    public decimal UnidadesVendidasHoy { get; set; }

    public List<VentaRow> UltimasVentas { get; set; } = new();

    public class VentaRow
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
