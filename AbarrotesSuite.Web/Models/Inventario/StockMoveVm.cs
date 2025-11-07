using System.ComponentModel.DataAnnotations;

namespace AbarrotesSuite.Web.Models.Inventario;

public class StockMoveVm
{
    [Required]
    public long ProductoId { get; set; }
    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Cantidad debe ser > 0")]
    public decimal Cantidad { get; set; }
    public string? Referencia { get; set; }
}
