using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Web.Models.Inventario;

namespace AbarrotesSuite.Web.Controllers;

[Authorize(Roles = "almacenero")]
public class InventarioController : Controller
{
    private readonly SalesDbContext _db;
    public InventarioController(SalesDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, bool soloActivos = true, bool bajoStock = false, int page = 1, int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _db.Productos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.Nombre.Contains(q) || (p.Sku ?? "").Contains(q) || (p.Barcode ?? "").Contains(q));
        }
        if (soloActivos) query = query.Where(p => p.Activo);
        if (bajoStock) query = query.Where(p => p.Stock <= 5);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListVm.ItemRow
            {
                Id = p.Id,
                Sku = p.Sku,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Costo = p.Costo,
                Stock = p.Stock,
                Activo = p.Activo
            })
            .ToListAsync();

        var vm = new ProductListVm
        {
            Q = q,
            SoloActivos = soloActivos,
            BajoStock = bajoStock,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var p = await _db.Productos.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EditProductVm
            {
                Id = x.Id,
                Sku = x.Sku,
                Barcode = x.Barcode,
                Nombre = x.Nombre,
                Precio = x.Precio,
                Costo = x.Costo,
                UnidadId = x.UnidadId,
                CategoriaId = x.CategoriaId,
                Activo = x.Activo
            })
            .FirstOrDefaultAsync();
        if (p == null) return NotFound();

        p.Unidades = await _db.Unidades.AsNoTracking()
            .OrderBy(u => u.Codigo)
            .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = u.Id.ToString(), Text = u.Codigo })
            .ToListAsync();
        p.Categorias = await _db.Categorias.AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
            .ToListAsync();
        p.Categorias.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "", Text = "-- Sin categoría --" });

        return View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProductVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCombos(vm);
            return View(vm);
        }

        // Validar SKU único si cambia
        if (!string.IsNullOrWhiteSpace(vm.Sku))
        {
            var exists = await _db.Productos.AsNoTracking()
                .AnyAsync(p => p.Sku == vm.Sku && p.Id != vm.Id);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Sku), "El SKU ya existe.");
                await FillCombos(vm);
                return View(vm);
            }
        }

        var p = await _db.Productos.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (p == null) return NotFound();

        p.Sku = vm.Sku;
        p.Barcode = vm.Barcode;
        p.Nombre = vm.Nombre;
        p.Precio = vm.Precio;
        p.Costo = vm.Costo;
        p.UnidadId = vm.UnidadId;
        p.CategoriaId = vm.CategoriaId;
        p.Activo = vm.Activo;

        await _db.SaveChangesAsync();
        TempData["msg"] = "Producto actualizado";
        return RedirectToAction(nameof(Index));
    }

    private async Task FillCombos(EditProductVm vm)
    {
        vm.Unidades = await _db.Unidades.AsNoTracking()
            .OrderBy(u => u.Codigo)
            .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = u.Id.ToString(), Text = u.Codigo })
            .ToListAsync();
        vm.Categorias = await _db.Categorias.AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
            .ToListAsync();
        vm.Categorias.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "", Text = "-- Sin categoría --" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IngresarStock(StockMoveVm vm)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var p = await _db.Productos.FirstOrDefaultAsync(x => x.Id == vm.ProductoId);
        if (p == null) return NotFound();
        if (vm.Cantidad <= 0) { ModelState.AddModelError(nameof(vm.Cantidad), "Cantidad debe ser > 0"); return BadRequest(ModelState); }

        using var tx = await _db.Database.BeginTransactionAsync();
        p.Stock += vm.Cantidad;
        _db.MovimientosStocks.Add(new MovimientosStock
        {
            ProductoId = p.Id,
            Tipo = "I",
            Cantidad = vm.Cantidad,
            Referencia = vm.Referencia
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["msg"] = "Ingreso de stock registrado";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjusteNegativo(StockMoveVm vm)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var p = await _db.Productos.FirstOrDefaultAsync(x => x.Id == vm.ProductoId);
        if (p == null) return NotFound();
        if (vm.Cantidad <= 0) { ModelState.AddModelError(nameof(vm.Cantidad), "Cantidad debe ser > 0"); return BadRequest(ModelState); }
        if (p.Stock < vm.Cantidad)
        {
            ModelState.AddModelError(nameof(vm.Cantidad), "Stock insuficiente para ajuste negativo");
            return BadRequest(ModelState);
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        p.Stock -= vm.Cantidad;
        _db.MovimientosStocks.Add(new MovimientosStock
        {
            ProductoId = p.Id,
            Tipo = "E",
            Cantidad = vm.Cantidad,
            Referencia = vm.Referencia
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["msg"] = "Ajuste negativo registrado";
        return RedirectToAction(nameof(Index));
    }
}
