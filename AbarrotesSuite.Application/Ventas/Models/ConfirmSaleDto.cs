namespace AbarrotesSuite.Application.Ventas.Models;

public class ConfirmSaleDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    // Añadido para reflejar pagos enviados desde Web
    public List<PaymentDto> Pagos { get; set; } = new();
}
