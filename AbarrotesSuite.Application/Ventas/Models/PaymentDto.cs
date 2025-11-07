namespace AbarrotesSuite.Application.Ventas.Models;

public class PaymentDto
{
    public string Metodo { get; set; } = "Efectivo"; // Efectivo/Tarjeta/etc
    public decimal Monto { get; set; }
}
