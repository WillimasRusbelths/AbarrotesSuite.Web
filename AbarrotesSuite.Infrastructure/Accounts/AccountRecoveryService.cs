using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Application.Accounts;
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using AbarrotesSuite.Infrastructure.Persistence.Extras;
using AbarrotesSuite.Domain.Identity;
using BC = BCrypt.Net.BCrypt;

namespace AbarrotesSuite.Infrastructure.Accounts;

public class AccountRecoveryService : IAccountRecoveryService
{
    private readonly SalesDbContext _db;
    public AccountRecoveryService(SalesDbContext db) => _db = db;

    private static AccountRecoveryDto Map(AccountRecovery r) => new()
    {
        Id = r.Id,
        EmpleadoId = r.EmpleadoId,
        UsernameInput = r.UsernameInput,
        Documento = r.Documento,
        Telefono = r.Telefono,
        Email = r.Email,
        RolSolicitado = r.RolSolicitado,
        Motivo = r.Motivo,
        Estado = r.Estado,
        CreadoEn = r.CreadoEn,
        RevisadoPor = r.RevisadoPor,
        RevisadoEn = r.RevisadoEn,
        NotasAdmin = r.NotasAdmin
    };

    public async Task<long> CreateRequestAsync(string? username, string? documento, string? telefono,
                                               string? email, string? rolSolicitado, string? motivo)
    {
        var r = new AccountRecovery
        {
            UsernameInput = username?.Trim(),
            Documento = documento?.Trim(),
            Telefono = telefono?.Trim(),
            Email = email?.Trim(),
            RolSolicitado = rolSolicitado,
            Motivo = motivo,
            Estado = "PENDIENTE",
            CreadoEn = DateTime.UtcNow
        };
        _db.AccountRecoveries.Add(r);
        await _db.SaveChangesAsync();
        return r.Id;
    }

    public async Task<List<AccountRecoveryDto>> ListAsync(string estado = "PENDIENTE")
        => await _db.AccountRecoveries.Where(x => x.Estado == estado)
            .OrderByDescending(x => x.CreadoEn)
            .Select(x => Map(x)).ToListAsync();

    public async Task<AccountRecoveryDto?> GetAsync(long id)
    {
        var r = await _db.AccountRecoveries.FirstOrDefaultAsync(x => x.Id == id);
        return r == null ? null : Map(r);
    }

    public async Task<(bool ok, string? error)> RejectAsync(long requestId, long adminUserId, string? notes)
    {
        var r = await _db.AccountRecoveries.FirstOrDefaultAsync(x => x.Id == requestId);
        if (r is null || r.Estado != "PENDIENTE") return (false, "Solicitud no válida.");
        r.Estado = "RECHAZADA";
        r.RevisadoPor = adminUserId;
        r.RevisadoEn = DateTime.UtcNow;
        r.NotasAdmin = notes;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ok, string? error, string tempUsername, string tempPassword)> ApproveAsync(
        long requestId, long adminUserId, bool disableOldUser, bool createIfMissing)
    {
        string tempUsername = string.Empty;
        string tempPassword = string.Empty;

        var r = await _db.AccountRecoveries.FirstOrDefaultAsync(x => x.Id == requestId);
        if (r is null || r.Estado != "PENDIENTE") return (false, "Solicitud no válida.", tempUsername, tempPassword);

        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Username == r.UsernameInput);
        if (u == null && !createIfMissing)
            return (false, "No existe el usuario, habilita 'createIfMissing' para crearlo.", tempUsername, tempPassword);
        if (u == null)
        {
            u = new Usuario
            {
                Username = string.IsNullOrWhiteSpace(r.Email) ? (r.UsernameInput ?? $"usr{DateTime.UtcNow.Ticks}") : r.Email!,
                HashPassword = string.Empty,
                Activo = true,
                CreadoEn = DateTime.UtcNow,
            };
            _db.Usuarios.Add(u);
            await _db.SaveChangesAsync();
        }
        if (disableOldUser)
            u.Activo = true; // ajustar lógica si se desea desactivar

        tempPassword = $"Tmp-{Guid.NewGuid():N}".Substring(0, 10);
        u.HashPassword = BC.HashPassword(tempPassword);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(r.RolSolicitado))
        {
            var rol = await _db.Roles.FirstOrDefaultAsync(x => x.Nombre == r.RolSolicitado);
            if (rol == null)
            {
                rol = new Role { Nombre = r.RolSolicitado! };
                _db.Roles.Add(rol);
                await _db.SaveChangesAsync();
            }
            var ya = await _db.Usuarios.Where(x => x.Id == u.Id).SelectMany(x => x.Rols).AnyAsync(x => x.Id == rol.Id);
            if (!ya)
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO ventas.usuario_roles(usuario_id, rol_id) VALUES ({0},{1})", u.Id, rol.Id);
            }
        }

        r.Estado = "APROBADA";
        r.RevisadoPor = adminUserId;
        r.RevisadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        tempUsername = u.Username;
        return (true, null, tempUsername, tempPassword);
    }
}
