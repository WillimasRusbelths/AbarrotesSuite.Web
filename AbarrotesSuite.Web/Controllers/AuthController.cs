using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AbarrotesSuite.Application.Auth;
using AbarrotesSuite.Web.Models.Auth;
using AbarrotesSuite.Application.Accounts;

public class AuthController : Controller
{
    private readonly IAuthService _auth;
    private readonly IAccountRecoveryService _recovery;
    public AuthController(IAuthService auth, IAccountRecoveryService recovery)
    { _auth = auth; _recovery = recovery; }

    private IActionResult RedirectByRole(IEnumerable<string> roles)
    {
        var set = roles?.Select(r => r.ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
        if (set.Contains("admin")) return RedirectToAction("Index", "Admin");
        if (set.Contains("cajero")) return RedirectToAction("Index", "Cajero");
        if (set.Contains("supervisor")) return RedirectToAction("Index", "Supervisor");
        if (set.Contains("almacenero")) return RedirectToAction("Index", "Inventario");
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            return RedirectByRole(roles);
        }
        ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return View(new LoginVm());
    }

    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (!ModelState.IsValid) return View(vm);

        var (ok, userId, userName, roles, error) = await _auth.ValidateAsync(vm.Username, vm.Password);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Usuario o contraseña inválidos");
            return View(vm);
        }

        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName)
        };
        foreach (var r in roles.Distinct()) claims.Add(new Claim(ClaimTypes.Role, r));

        var props = new AuthenticationProperties
        {
            IsPersistent = vm.RememberMe,
            ExpiresUtc = vm.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
        };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), props);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectByRole(roles);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete(".AspNetCore.Cookies");
        return RedirectToAction("Login", "Auth");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Denied() => View();

    // Recovery public endpoints
    [AllowAnonymous]
    [HttpGet]
    public IActionResult RequestRecovery() => View(new AccountRecoveryRequestVm());

    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRecovery(AccountRecoveryRequestVm vm)
    {
        await _recovery.CreateRequestAsync(vm.Username, vm.Documento, vm.Telefono, vm.Email, vm.RolSolicitado, vm.Motivo);
        TempData["msg"] = "Solicitud enviada. Un administrador la revisará.";
        return RedirectToAction(nameof(Login));
    }

    // Semilla rápida: /Auth/SeedAdmin?u=admin&p=admin123
    [HttpGet]
    public async Task<IActionResult> SeedAdmin(string u = "admin", string p = "admin123")
    {
        var (ok, err) = await _auth.EnsureAdminAsync(u, p);
        return Content(ok ? $"Admin listo: {u}" : $"Error: {err}");
    }
}
