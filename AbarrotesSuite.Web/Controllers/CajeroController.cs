using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Web.Models.Cajero;

namespace AbarrotesSuite.Web.Controllers;

[Authorize(Roles = "cajero")]
public class CajeroController : Controller
{
    private readonly SalesDbContext _db;
    public CajeroController(SalesDbContext db) => _db = db;

    private static (DateTime startUtc, DateTime endUtc) TodayUtcForLima()
    {
        var off = TimeSpan.FromHours(-5);
        var nowUtc = DateTime.UtcNow;
        var startLocal = (nowUtc + off).Date;
        var endLocal = startLocal.AddDays(1);
        return (startLocal - off, endLocal - off);
    }

    public async Task<IActionResult> Index()
    {
        var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(uidStr, out var uid)) return Unauthorized();

        var (startUtc, endUtc) = TodayUtcForLima();

        var ventasQ = _db.Ventas.AsNoTracking()
            .Where(v => v.UsuarioId == uid && v.Fecha >= startUtc && v.Fecha < endUtc);

        var ticketsHoy = await ventasQ.Where(v => v.Estado == "ACTIVA").CountAsync();
        var montoHoy = await ventasQ.Where(v => v.Estado == "ACTIVA").SumAsync(v => (decimal?)v.Total) ?? 0m;

        var unidadesVendidasHoy = await _db.VentasDetalles.AsNoTracking()
            .Where(d => d.Venta.UsuarioId == uid && d.Venta.Fecha >= startUtc && d.Venta.Fecha < endUtc && d.Venta.Estado == "ACTIVA")
            .SumAsync(d => (decimal?)d.Cantidad) ?? 0m;

        var ultimasVentas = await ventasQ
            .OrderByDescending(v => v.Fecha)
            .Take(10)
            .Select(v => new CashierDashboardVm.VentaRow
            {
                Id = v.Id,
                Fecha = v.Fecha,
                Total = v.Total,
                Estado = v.Estado
            })
            .ToListAsync();

        var vm = new CashierDashboardVm
        {
            TicketsHoy = ticketsHoy,
            MontoHoy = montoHoy,
            UnidadesVendidasHoy = unidadesVendidasHoy,
            UltimasVentas = ultimasVentas
        };
        return View(vm);
    }
}
