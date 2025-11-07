using System.ComponentModel.DataAnnotations;

namespace AbarrotesSuite.Web.Models.Supervisor;

public class ReportsVm
{
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public int Tickets { get; set; }
    public decimal Monto { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal Unidades { get; set; }
    public int Anuladas { get; set; }

    public List<RowKpi> VentasPorUsuario { get; set; } = new();
    public List<RowKpi> PagosPorMetodo { get; set; } = new();
    public List<RowProd> TopUnidades { get; set; } = new();
    public List<RowProd> TopMonto { get; set; } = new();

    public class RowKpi { public string Clave { get; set; } = string.Empty; public decimal Monto { get; set; } public int Cantidad { get; set; } }
    public class RowProd { public long ProductoId { get; set; } public string? Sku { get; set; } public string Nombre { get; set; } = string.Empty; public decimal Valor { get; set; } }
}
