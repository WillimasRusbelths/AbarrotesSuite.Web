using AbarrotesSuite.Application.Auth;
using AbarrotesSuite.Domain.Identity;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using Microsoft.EntityFrameworkCore;
// Alias para que sea claro el uso de BCrypt
using BC = BCrypt.Net.BCrypt;

namespace AbarrotesSuite.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly SalesDbContext _db;
    public AuthService(SalesDbContext db) => _db = db;

    public async Task<(bool ok, long userId, string userName, List<string> roles, string? error)>
        ValidateAsync(string username, string password)
    {
        // Clases y propiedades PascalCase que generó el scaffold:
        // Usuario { Id, Username, HashPassword, Activo, ... }
        // Role    { Id, Nombre }
        // Relación muchos a muchos Usuario <-> Role vía navegación Usuario.Rols
        var u = await _db.Usuarios
            .FirstOrDefaultAsync(x => x.Username == username && x.Activo == true);

        if (u == null) return (false, 0, "", new(), "Usuario o contraseña inválidos");

        if (!BC.Verify(password, u.HashPassword))
            return (false, 0, "", new(), "Usuario o contraseña inválidos");

        var roles = await _db.Usuarios
            .Where(x => x.Id == u.Id)
            .SelectMany(x => x.Rols.Select(r => r.Nombre))
            .ToListAsync();

        return (true, u.Id, u.Username, roles, null);
    }

    public async Task<(bool ok, string? error)> EnsureAdminAsync(string username, string password)
    {
        var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == Roles.Admin);
        if (adminRole == null)
        {
            adminRole = new Role { Nombre = Roles.Admin };
            _db.Roles.Add(adminRole);
            await _db.SaveChangesAsync();
        }

        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Username == username);
        if (u == null)
        {
            u = new Usuario
            {
                Username = username,
                HashPassword = BC.HashPassword(password),
                Activo = true,
                CreadoEn = DateTime.UtcNow
            };
            _db.Usuarios.Add(u);
            await _db.SaveChangesAsync();
        }

        // Verificar si el usuario ya tiene el rol admin
        var tiene = await _db.Entry(u)
            .Collection(x => x.Rols)
            .Query()
            .AnyAsync(r => r.Id == adminRole.Id);

        if (!tiene)
        {
            // Agregar el rol a la colección de roles del usuario
            u.Rols.Add(adminRole);
            await _db.SaveChangesAsync();
        }

        return (true, null);
    }

    public async Task<(bool ok, string? error)> RegisterAsync(string username, string password, string roleName = "cajero")
    {
        if (await _db.Usuarios.AnyAsync(x => x.Username == username))
            return (false, "El usuario ya existe");

        var user = new Usuario
        {
            Username = username,
            HashPassword = BC.HashPassword(password),
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();

        var rol = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == roleName);
        if (rol == null)
        {
            rol = new Role { Nombre = roleName };
            _db.Roles.Add(rol);
            await _db.SaveChangesAsync();
        }

        // Asociar el rol al usuario mediante la navegación muchos a muchos
        user.Rols.Add(rol);
        await _db.SaveChangesAsync();

        return (true, null);
    }
}
