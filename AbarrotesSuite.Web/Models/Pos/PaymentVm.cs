namespace AbarrotesSuite.Web.Models.Pos;

public class PaymentVm
{
    public string Metodo { get; set; } = "Efectivo"; // Efectivo/Tarjeta/Mixto
    public decimal Monto { get; set; }
}

