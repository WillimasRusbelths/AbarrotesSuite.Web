using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Domain.Identity;
using AbarrotesSuite.Application.Auth;
using AbarrotesSuite.Web.Models.Admin;
using AbarrotesSuite.Application.Accounts;
using AbarrotesSuite.Web.Models.Auth;


[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IAuthService _auth;
    private readonly SalesDbContext _db;
    private readonly IAccountRecoveryService _recovery;
    public AdminController(IAuthService auth, SalesDbContext db, IAccountRecoveryService recovery)
    {
        _auth = auth;
        _db = db;
        _recovery = recovery;
    }

    // Utilidad local para obtener hoy en UTC respecto a Lima (UTC-5)
    private static (DateTime startUtc, DateTime endUtc) TodayUtcForLima()
    {
        var off = TimeSpan.FromHours(-5);
        var nowUtc = DateTime.UtcNow;
        var startLocal = (nowUtc + off).Date; // inicio del día en Lima
        var endLocal = startLocal.AddDays(1);
        return (startLocal - off, endLocal - off);
    }

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        var (startUtc, endUtc) = TodayUtcForLima();

        // Consultas base (sin paralelismo) y siempre AsNoTracking
        var ventasDiaQ = _db.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= startUtc && v.Fecha < endUtc);
        var ventasActivasDiaQ = ventasDiaQ.Where(v => v.Estado == "ACTIVA");

        // KPIs
        var ticketsHoy = await ventasActivasDiaQ.CountAsync();
        var montoHoy = await ventasActivasDiaQ.SumAsync(v => (decimal?)v.Total) ?? 0m;
        var ticketPromedio = ticketsHoy > 0 ? decimal.Round(montoHoy / ticketsHoy, 2) : 0m;

        var unidadesVendidasHoy = await _db.VentasDetalles.AsNoTracking()
            .Where(d => d.Venta.Fecha >= startUtc && d.Venta.Fecha < endUtc && d.Venta.Estado == "ACTIVA")
            .SumAsync(d => (decimal?)d.Cantidad) ?? 0m;

        // Caja del día por método (pagos de ventas activas del día)
        var cajaPorMetodo = await _db.Pagos.AsNoTracking()
            .Where(p => p.Venta.Fecha >= startUtc && p.Venta.Fecha < endUtc && p.Venta.Estado == "ACTIVA")
            .GroupBy(p => p.Metodo)
            .Select(g => new DashboardVm.CajaMetodoRow { Metodo = g.Key, Monto = g.Sum(x => x.Monto) })
            .OrderByDescending(x => x.Monto)
            .ToListAsync();
        var cajaTotal = cajaPorMetodo.Sum(x => x.Monto);

        // Últimas ventas (Top 10) con usuario
        var ultimasVentas = await ventasDiaQ
            .OrderByDescending(v => v.Fecha)
            .Take(10)
            .Select(v => new DashboardVm.VentaRow
            {
                Id = v.Id,
                Fecha = v.Fecha,
                Total = v.Total,
                Estado = v.Estado,
                Usuario = v.Usuario.Username
            })
            .ToListAsync();

        // Usuarios recientes (Top 5 del día)
        var usuariosRecientes = await _db.Usuarios.AsNoTracking()
            .Where(u => u.CreadoEn >= startUtc && u.CreadoEn < endUtc)
            .OrderByDescending(u => u.CreadoEn)
            .Take(5)
            .Select(u => new DashboardVm.UsuarioRow
            {
                Id = u.Id,
                Username = u.Username,
                CreadoEn = u.CreadoEn,
                Activo = u.Activo
            })
            .ToListAsync();

        // Bajo stock (<= 5) Top 10
        var bajoStock = await _db.Productos.AsNoTracking()
            .Where(p => p.Activo && p.Stock <= 5)
            .OrderBy(p => p.Stock).ThenBy(p => p.Nombre)
            .Take(10)
            .Select(p => new DashboardVm.ProductoRow
            {
                Id = p.Id,
                Sku = p.Sku,
                Nombre = p.Nombre,
                Stock = p.Stock,
                Unidades = null,
                Monto = null
            })
            .ToListAsync();

        // Anuladas hoy
        var anuladasHoy = await ventasDiaQ.Where(v => v.Estado == "ANULADA").CountAsync();

        var vm = new DashboardVm
        {
            TicketsHoy = ticketsHoy,
            MontoHoy = montoHoy,
            TicketPromedio = ticketPromedio,
            UnidadesVendidasHoy = unidadesVendidasHoy,
            CajaPorMetodo = cajaPorMetodo,
            CajaTotal = cajaTotal,
            UltimasVentas = ultimasVentas,
            UsuariosRecientes = usuariosRecientes,
            BajoStock = bajoStock,
            AnuladasHoy = anuladasHoy
        };

        return View(vm);
    }

    // GET: /Admin/CreateUser
    [HttpGet]
    public IActionResult CreateUser()
    {
        ViewBag.Roles = new SelectList(new[] { Roles.Admin, Roles.Cajero, Roles.Supervisor, Roles.Almacenero });
        return View(new CreateUserVm());
    }

    // POST: /Admin/CreateUser
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserVm vm)
    {
        ViewBag.Roles = new SelectList(new[] { Roles.Admin, Roles.Cajero, Roles.Supervisor, Roles.Almacenero });
        if (!ModelState.IsValid) return View(vm);

        var (ok, err) = await _auth.RegisterAsync(vm.Username.Trim(), vm.Password, vm.Role);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, err ?? "No se pudo crear el usuario");
            return View(vm);
        }

        TempData["msg"] = $"Usuario '{vm.Username}' creado con rol '{vm.Role}'.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/RecoveryRequests
    [HttpGet]
    public async Task<IActionResult> RecoveryRequests(string estado = "PENDIENTE")
    {
        var list = await _recovery.ListAsync(estado);
        return View(list);
    }

    // POST: /Admin/ApproveRecovery
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRecovery(RecoveryApproveVm vm)
    {
        var adminId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var (ok, err, user, pass) = await _recovery.ApproveAsync(vm.Id, adminId, vm.DisableOldUser, vm.CreateIfMissing);
        if (!ok) { TempData["err"] = err; }
        else     { TempData["msg"] = $"Aprobada. Usuario: {user} | Contraseña temporal: {pass}"; }
        return RedirectToAction(nameof(RecoveryRequests));
    }

    // POST: /Admin/RejectRecovery
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRecovery(long id, string? notes)
    {
        var adminId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var (ok, err) = await _recovery.RejectAsync(id, adminId, notes);
        if (!ok) TempData["err"] = err; else TempData["msg"] = "Solicitud rechazada.";
        return RedirectToAction(nameof(RecoveryRequests));
    }
}
