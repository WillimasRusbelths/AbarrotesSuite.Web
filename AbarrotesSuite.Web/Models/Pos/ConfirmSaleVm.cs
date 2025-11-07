namespace AbarrotesSuite.Web.Models.Pos;

public class ConfirmSaleVm
{
    public List<CartItemVm> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string? Observacion { get; set; }

    // NUEVO:
    public List<PaymentVm> Pagos { get; set; } = new();
}
