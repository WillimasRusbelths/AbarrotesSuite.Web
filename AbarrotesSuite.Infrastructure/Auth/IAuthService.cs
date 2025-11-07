namespace AbarrotesSuite.Application.Auth;

public interface IAuthService
{
    Task<(bool ok, long userId, string userName, List<string> roles, string? error)>
        ValidateAsync(string username, string password);

    Task<(bool ok, string? error)>
        EnsureAdminAsync(string username, string password);

    Task<(bool ok, string? error)>
        RegisterAsync(string username, string password, string roleName = "cajero"); // lo usaremos en la parte 2
}
