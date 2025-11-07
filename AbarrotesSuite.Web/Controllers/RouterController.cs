using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AbarrotesSuite.Domain.Identity;

[Route("router")]
public class RouterController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
            return RedirectToAction("Login", "Auth");

        if (User.IsInRole(Roles.Admin)) return RedirectToAction("Index", "Admin");
        if (User.IsInRole(Roles.Cajero)) return RedirectToAction("Pos", "Ventas");
        if (User.IsInRole(Roles.Almacenero)) return RedirectToAction("Index", "Inventario");
        if (User.IsInRole(Roles.Supervisor)) return RedirectToAction("Index", "Supervisor");

        return RedirectToAction("Denied", "Auth");
    }
}
