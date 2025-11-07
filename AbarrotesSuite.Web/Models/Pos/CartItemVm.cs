namespace AbarrotesSuite.Web.Models.Pos;

public class CartItemVm
{
    public long ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }   // usa decimal si vendes por peso
    public decimal Subtotal => decimal.Round(PrecioUnitario * Cantidad, 2);
}
