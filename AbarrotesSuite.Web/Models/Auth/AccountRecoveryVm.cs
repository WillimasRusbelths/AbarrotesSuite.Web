namespace AbarrotesSuite.Web.Models.Auth;

public class AccountRecoveryRequestVm
{
    public string? Username { get; set; }
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? RolSolicitado { get; set; }
    public string? Motivo { get; set; }
}

public class RecoveryApproveVm
{
    public long Id { get; set; }
    public bool DisableOldUser { get; set; } = false;
    public bool CreateIfMissing { get; set; } = true;
    public string? Notes { get; set; }
}
