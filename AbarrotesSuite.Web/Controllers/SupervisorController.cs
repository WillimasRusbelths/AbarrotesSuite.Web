using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Web.Models.Supervisor;
using System.Text;

namespace AbarrotesSuite.Web.Controllers;

[Authorize(Roles = "supervisor")]
public class SupervisorController : Controller
{
    private readonly SalesDbContext _db;
    public SupervisorController(SalesDbContext db) => _db = db;

    private static (DateTime startUtc, DateTime endUtc) ToUtcRange(DateTime startDate, DateTime endDate)
    {
        var off = TimeSpan.FromHours(-5); // Lima -05
        var startUtc = startDate.Date - off;
        var endUtc = endDate.Date.AddDays(1) - off; // inclusive día completo
        return (startUtc, endUtc);
    }

    [HttpGet]
    public IActionResult Index()
    {
        var todayLocal = DateTime.UtcNow.AddHours(-5).Date; // Lima date
        var vm = new ReportsVm
        {
            StartDate = todayLocal,
            EndDate = todayLocal
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ReportsVm vm)
    {
        if (vm.StartDate > vm.EndDate)
        {
            ModelState.AddModelError(string.Empty, "Rango inválido");
            return View(vm);
        }

        var (startUtc, endUtc) = ToUtcRange(vm.StartDate, vm.EndDate);

        var ventasQ = _db.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= startUtc && v.Fecha < endUtc);
        var ventasActivasQ = ventasQ.Where(v => v.Estado == "ACTIVA");

        vm.Tickets = await ventasActivasQ.CountAsync();
        vm.Monto = await ventasActivasQ.SumAsync(v => (decimal?)v.Total) ?? 0m;
        vm.TicketPromedio = vm.Tickets > 0 ? decimal.Round(vm.Monto / vm.Tickets, 2) : 0m;
        vm.Unidades = await _db.VentasDetalles.AsNoTracking()
            .Where(d => d.Venta.Fecha >= startUtc && d.Venta.Fecha < endUtc && d.Venta.Estado == "ACTIVA")
            .SumAsync(d => (decimal?)d.Cantidad) ?? 0m;
        vm.Anuladas = await ventasQ.CountAsync(v => v.Estado == "ANULADA");

        vm.VentasPorUsuario = await ventasActivasQ
            .GroupBy(v => new { v.UsuarioId, v.Usuario.Username })
            .Select(g => new ReportsVm.RowKpi
            {
                Clave = g.Key.Username,
                Monto = g.Sum(x => x.Total),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Monto)
            .Take(20)
            .ToListAsync();

        vm.PagosPorMetodo = await _db.Pagos.AsNoTracking()
            .Where(p => p.Venta.Fecha >= startUtc && p.Venta.Fecha < endUtc && p.Venta.Estado == "ACTIVA")
            .GroupBy(p => p.Metodo)
            .Select(g => new ReportsVm.RowKpi
            {
                Clave = g.Key,
                Monto = g.Sum(x => x.Monto),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Monto)
            .ToListAsync();

        vm.TopUnidades = await _db.VentasDetalles.AsNoTracking()
            .Where(d => d.Venta.Fecha >= startUtc && d.Venta.Fecha < endUtc && d.Venta.Estado == "ACTIVA")
            .GroupBy(d => new { d.ProductoId, d.Producto.Sku, d.Producto.Nombre })
            .Select(g => new ReportsVm.RowProd
            {
                ProductoId = g.Key.ProductoId,
                Sku = g.Key.Sku,
                Nombre = g.Key.Nombre,
                Valor = g.Sum(x => x.Cantidad)
            })
            .OrderByDescending(x => x.Valor)
            .Take(20)
            .ToListAsync();

        vm.TopMonto = await _db.VentasDetalles.AsNoTracking()
            .Where(d => d.Venta.Fecha >= startUtc && d.Venta.Fecha < endUtc && d.Venta.Estado == "ACTIVA")
            .GroupBy(d => new { d.ProductoId, d.Producto.Sku, d.Producto.Nombre })
            .Select(g => new ReportsVm.RowProd
            {
                ProductoId = g.Key.ProductoId,
                Sku = g.Key.Sku,
                Nombre = g.Key.Nombre,
                Valor = g.Sum(x => (decimal?)(x.Importe ?? x.Cantidad * x.Precio)) ?? 0m
            })
            .OrderByDescending(x => x.Valor)
            .Take(20)
            .ToListAsync();

        return View(vm);
    }

    private FileContentResult Csv(string filename, IEnumerable<string> lines)
    {
        var data = Encoding.UTF8.GetBytes(string.Join("\n", lines));
        return File(data, "text/csv; charset=utf-8", filename);
    }

    [HttpGet]
    public async Task<FileContentResult> ExportUsuariosCsv(DateTime startDate, DateTime endDate)
    {
        var (startUtc, endUtc) = ToUtcRange(startDate, endDate);
        var ventasActivasQ = _db.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= startUtc && v.Fecha < endUtc && v.Estado == "ACTIVA");
        var rows = await ventasActivasQ
            .GroupBy(v => new { v.UsuarioId, v.Usuario.Username })
            .Select(g => new { g.Key.Username, Monto = g.Sum(x => x.Total), Cant = g.Count() })
            .OrderByDescending(x => x.Monto)
            .ToListAsync();
        var lines = new List<string> { "Usuario,Monto,Cantidad" };
        lines.AddRange(rows.Select(r => $"{r.Username},{r.Monto.ToString("N2")},{r.Cant}"));
        return Csv($"ventas_usuarios_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv", lines);
    }

    [HttpGet]
    public async Task<FileContentResult> ExportPagosCsv(DateTime startDate, DateTime endDate)
    {
        var (startUtc, endUtc) = ToUtcRange(startDate, endDate);
        var pagosQ = _db.Pagos.AsNoTracking()
            .Where(p => p.Venta.Fecha >= startUtc && p.Venta.Fecha < endUtc && p.Venta.Estado == "ACTIVA");
        var rows = await pagosQ
            .GroupBy(p => p.Metodo)
            .Select(g => new { Metodo = g.Key, Monto = g.Sum(x => x.Monto), Cant = g.Count() })
            .OrderByDescending(x => x.Monto)
            .ToListAsync();
        var lines = new List<string> { "Metodo,Monto,Cantidad" };
        lines.AddRange(rows.Select(r => $"{r.Metodo},{r.Monto.ToString("N2")},{r.Cant}"));
        return Csv($"pagos_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv", lines);
    }

    [HttpGet]
    public async Task<FileContentResult> ExportTopProductosCsv(DateTime startDate, DateTime endDate, string tipo)
    {
        var (startUtc, endUtc) = ToUtcRange(startDate, endDate);
        var detQ = _db.VentasDetalles.AsNoTracking()
            .Where(d => d.Venta.Fecha >= startUtc && d.Venta.Fecha < endUtc && d.Venta.Estado == "ACTIVA");
        var lines = new List<string>();
        if (tipo == "unidades")
        {
            var rows = await detQ
                .GroupBy(d => new { d.ProductoId, d.Producto.Sku, d.Producto.Nombre })
                .Select(g => new { g.Key.Sku, g.Key.Nombre, Val = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.Val)
                .Take(100)
                .ToListAsync();
            lines.Add("SKU,Nombre,Unidades");
            lines.AddRange(rows.Select(r => $"{r.Sku},{r.Nombre},{r.Val.ToString("N2")}"));
            return Csv($"top_unidades_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv", lines);
        }
        else
        {
            var rows = await detQ
                .GroupBy(d => new { d.ProductoId, d.Producto.Sku, d.Producto.Nombre })
                .Select(g => new { g.Key.Sku, g.Key.Nombre, Val = g.Sum(x => (decimal?)(x.Importe ?? x.Cantidad * x.Precio)) ?? 0m })
                .OrderByDescending(x => x.Val)
                .Take(100)
                .ToListAsync();
            lines.Add("SKU,Nombre,Monto");
            lines.AddRange(rows.Select(r => $"{r.Sku},{r.Nombre},{r.Val.ToString("N2")}"));
            return Csv($"top_monto_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv", lines);
        }
    }
}
