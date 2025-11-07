namespace AbarrotesSuite.Application.Accounts;

public class AccountRecoveryDto
{
    public long Id { get; set; }
    public long? EmpleadoId { get; set; }
    public string? UsernameInput { get; set; }
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? RolSolicitado { get; set; }
    public string? Motivo { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime CreadoEn { get; set; }
    public long? RevisadoPor { get; set; }
    public DateTime? RevisadoEn { get; set; }
    public string? NotasAdmin { get; set; }
}
