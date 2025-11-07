using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Application.Auth;
using AbarrotesSuite.Infrastructure.Auth;
using AbarrotesSuite.Application.Ventas;
using AbarrotesSuite.Infrastructure.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AbarrotesSuite.Application.Accounts;
using AbarrotesSuite.Infrastructure.Accounts;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SalesDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));
builder.Services.AddScoped<IAuthService, AuthService>();
// Registro del servicio de recuperación de cuentas
builder.Services.AddScoped<IAccountRecoveryService, AccountRecoveryService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.AccessDeniedPath = "/Auth/Denied";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Events.OnRedirectToLogin = ctx =>
        {
            var path = ctx.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/Auth/Login", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask; // evita bucle
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Rutas: único dueño de "/" y una ruta MVC general
app.MapControllerRoute(
    name: "root",
    pattern: "/",
    defaults: new { controller = "Auth", action = "Login" });

app.MapControllerRoute(
    name: "mvc",
    pattern: "{controller}/{action}/{id?}");

app.Run();
