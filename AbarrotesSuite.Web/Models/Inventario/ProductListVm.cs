namespace AbarrotesSuite.Web.Models.Inventario;

public class ProductListVm
{
    public string? Q { get; set; }
    public bool SoloActivos { get; set; } = true;
    public bool BajoStock { get; set; } = false;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int Total { get; set; }

    public List<ItemRow> Items { get; set; } = new();

    public class ItemRow
    {
        public long Id { get; set; }
        public string? Sku { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal? Costo { get; set; }
        public decimal Stock { get; set; }
        public bool Activo { get; set; }
    }
}
