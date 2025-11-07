using System.Linq;
using AbarrotesSuite.Application.Ventas;
using AbarrotesSuite.Application.Ventas.Models; // ConfirmSaleDto, PaymentDto, CartItemDto
using AbarrotesSuite.Infrastructure.Persistence.Generated;
using Microsoft.EntityFrameworkCore;

namespace AbarrotesSuite.Infrastructure.Ventas;

public class SalesService : ISalesService
{
    private readonly SalesDbContext _db;
    public SalesService(SalesDbContext db) => _db = db;

    public async Task<long> CreateSaleAsync(long usuarioId, ConfirmSaleDto venta)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        // Fallback de pagos si no vienen
        venta.Pagos ??= new();
        if (venta.Pagos.Count == 0)
            venta.Pagos.Add(new PaymentDto { Metodo = "Efectivo", Monto = venta.Total });

        var pagado = venta.Pagos.Sum(p => p.Monto);
        var diff = pagado - venta.Total;
        if (diff < -0.01m) // tolerancia 1 centavo
            throw new InvalidOperationException("El total pagado es menor que el total de la venta.");

        var v = new Venta
        {
            Fecha = DateTime.UtcNow,
            UsuarioId = usuarioId,
            Total = venta.Total,
            Estado = "ACTIVA" // <- IMPORTANTE: coincide con CK (ACTIVA/ANULADA)
        };
        _db.Ventas.Add(v);
        await _db.SaveChangesAsync();

        foreach (var it in venta.Items)
        {
            var prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == it.ProductoId);
            if (prod == null) throw new InvalidOperationException("Producto no encontrado");

            _db.VentasDetalles.Add(new VentasDetalle
            {
                VentaId = v.Id,
                ProductoId = it.ProductoId,
                Cantidad = it.Cantidad,
                Precio = it.PrecioUnitario   // 'importe' lo calcula la BD
            });
        }

        foreach (var p in venta.Pagos)
        {
            _db.Pagos.Add(new Pago
            {
                VentaId = v.Id,
                Metodo = p.Metodo,  
                Monto = p.Monto,
                Fecha = DateTime.UtcNow 
            });
        }

        // 5 Guardar y confirmar transacción
        try
        {
            await _db.SaveChangesAsync(); // dispara trigger: valida y descuenta stock
            await tx.CommitAsync();
            return v.Id;
        }
        catch (DbUpdateException ex)
        {
            // Si el trigger reclamó stock
            if (ex.InnerException?.Message?.Contains("Stock insuficiente") == true)
                throw new InvalidOperationException("Stock insuficiente para uno o más productos.");
            throw;
        }
    }
}
