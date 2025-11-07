using AbarrotesSuite.Application.Ventas.Models; // mover VMs a Application para evitar dependencias con Web
namespace AbarrotesSuite.Application.Ventas;

public interface ISalesService
{
    Task<long> CreateSaleAsync(long usuarioId, ConfirmSaleDto venta);
}


