namespace AbarrotesSuite.Application.Ventas.Models;

public class CartItemDto
{
    public long ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
}
