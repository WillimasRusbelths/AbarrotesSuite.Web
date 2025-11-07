using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Domain.Identity;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Application.Ventas;
using AbarrotesSuite.Application.Ventas.Models;
using AbarrotesSuite.Web.Models.Pos;
using System.Security.Claims;

[Authorize(Roles = Roles.Cajero)]
public class VentasController : Controller
{
    private readonly SalesDbContext _db;
    private readonly ISalesService _sales;

    public VentasController(SalesDbContext db, ISalesService sales)
    {
        _db = db;
        _sales = sales;
    }

    // /Ventas/Pos
    public IActionResult Pos() => View();

    // /Ventas/Buscar?term=arroz
    [HttpGet]
    public async Task<IActionResult> Buscar(string term)
    {
        term = term?.Trim() ?? "";
        if (term.Length < 1) return Json(Array.Empty<object>());

        // Ajusta nombres de columnas: Sku, Nombre, Precio
        var q = await _db.Productos
            .Where(p => p.Activo == true && (p.Nombre.Contains(term) || p.Sku!.Contains(term)))
            .OrderBy(p => p.Nombre)
            .Select(p => new {
                id = p.Id,
                nombre = p.Nombre,
                sku = p.Sku,
                precio = p.Precio
            })
            .Take(15)
            .ToListAsync();

        return Json(q);
    }

    // /Ventas/Confirmar (POST JSON)
    [HttpPost]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmSaleVm vm)
    {
        if (vm == null || vm.Items.Count == 0) return BadRequest("Carrito vacío");

        var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(uidStr, out var uid)) return Unauthorized();

        // Mapear VM del Web a DTO de Application
        var dto = new ConfirmSaleDto
        {
            Total = vm.Total,
            Observacion = vm.Observacion,
            Items = vm.Items.Select(i => new CartItemDto
            {
                ProductoId = i.ProductoId,
                Nombre = i.Nombre,
                Sku = i.Sku,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad
            }).ToList(),
            Pagos = (vm.Pagos ?? new List<PaymentVm>()).Select(p => new PaymentDto
            {
                Metodo = p.Metodo,
                Monto = p.Monto
            }).ToList()
        };

        // Fallback: si no se envían pagos, asumir efectivo = total
        if (dto.Pagos == null || dto.Pagos.Count == 0)
        {
            dto.Pagos = new List<PaymentDto> { new PaymentDto { Metodo = "Efectivo", Monto = dto.Total } };
        }

        try
        {
            var id = await _sales.CreateSaleAsync(uid, dto);
            return Ok(new { ventaId = id });
        }
        catch (InvalidOperationException ex)
        {
            // Mensaje amigable al front (POS captura y muestra en alert)
            return BadRequest(ex.Message);
        }
    }
}
