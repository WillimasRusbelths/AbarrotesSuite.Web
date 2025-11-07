using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AbarrotesSuite.Web.Models.Inventario;

public class EditProductVm
{
    public long Id { get; set; }
    [Display(Name="SKU")]
    public string? Sku { get; set; }
    [Display(Name="Barcode")]
    public string? Barcode { get; set; }
    [Required]
    [Display(Name="Nombre")]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    [Display(Name="Precio")]
    public decimal Precio { get; set; }
    [Display(Name="Costo")]
    public decimal? Costo { get; set; }
    [Required]
    [Display(Name="Unidad")]
    public int UnidadId { get; set; }
    [Display(Name="Categoría")]
    public int? CategoriaId { get; set; }
    [Display(Name="Activo")]
    public bool Activo { get; set; }

    public List<SelectListItem> Unidades { get; set; } = new();
    public List<SelectListItem> Categorias { get; set; } = new();
}
