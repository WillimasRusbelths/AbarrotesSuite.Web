using System.Collections.Generic;

namespace AbarrotesSuite.Application.Accounts;

public interface IAccountRecoveryService
{
    Task<long> CreateRequestAsync(string? username, string? documento, string? telefono,
                                  string? email, string? rolSolicitado, string? motivo);

    Task<(bool ok, string? error, string tempUsername, string tempPassword)> ApproveAsync(
        long requestId, long adminUserId, bool disableOldUser, bool createIfMissing);

    Task<(bool ok, string? error)> RejectAsync(long requestId, long adminUserId, string? notes);

    Task<List<AccountRecoveryDto>> ListAsync(string estado = "PENDIENTE");
    Task<AccountRecoveryDto?> GetAsync(long id);
}
