using Microsoft.EntityFrameworkCore;
using AbarrotesSuite.Infrastructure.Persistence.Extras;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated
{
    public partial class SalesDbContext
    {
        public virtual DbSet<AccountRecovery> AccountRecoveries { get; set; } = null!;

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountRecovery>(e =>
            {
                e.ToTable("recuperacion_cuenta", "ventas");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.EmpleadoId).HasColumnName("empleado_id");
                e.Property(x => x.UsernameInput).HasColumnName("username_input");
                e.Property(x => x.Documento).HasColumnName("documento");
                e.Property(x => x.Telefono).HasColumnName("telefono");
                e.Property(x => x.Email).HasColumnName("email");
                e.Property(x => x.RolSolicitado).HasColumnName("rol_solicitado");
                e.Property(x => x.Motivo).HasColumnName("motivo");
                e.Property(x => x.Estado).HasColumnName("estado");
                e.Property(x => x.CreadoEn).HasColumnName("creado_en");
                e.Property(x => x.RevisadoPor).HasColumnName("revisado_por");
                e.Property(x => x.RevisadoEn).HasColumnName("revisado_en");
                e.Property(x => x.NotasAdmin).HasColumnName("notas_admin");
            });
        }
    }
}
