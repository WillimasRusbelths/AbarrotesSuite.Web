using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AbarrotesSuite.Infrastructure.Persistence.Generated;

public partial class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Conversacione> Conversaciones { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Mensaje> Mensajes { get; set; }

    public virtual DbSet<MovimientosStock> MovimientosStocks { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Unidade> Unidades { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<VentasDetalle> VentasDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3213E83F98205B1A");

            entity.ToTable("categorias", "ventas");

            entity.HasIndex(e => e.Nombre, "UQ__categori__72AFBCC68A43FB4B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Conversacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conversa__3213E83F0EFFC55F");

            entity.ToTable("conversaciones", "chatbot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreadoEn)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__empleado__3213E83FA2B3E344");

            entity.ToTable("empleados", "ventas");

            entity.HasIndex(e => e.Documento, "UQ__empleado__A25B3E6177A8D3D7").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(80)
                .HasColumnName("apellidos");
            entity.Property(e => e.Cargo)
                .HasMaxLength(40)
                .HasColumnName("cargo");
            entity.Property(e => e.CreadoEn)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Documento)
                .HasMaxLength(32)
                .HasColumnName("documento");
            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .HasColumnName("email");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.Nombres)
                .HasMaxLength(80)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__mensajes__3213E83FC6C9309C");

            entity.ToTable("mensajes", "chatbot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConversacionId).HasColumnName("conversacion_id");
            entity.Property(e => e.Rol)
                .HasMaxLength(10)
                .HasColumnName("rol");
            entity.Property(e => e.Texto).HasColumnName("texto");
            entity.Property(e => e.Ts)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ts");

            entity.HasOne(d => d.Conversacion).WithMany(p => p.Mensajes)
                .HasForeignKey(d => d.ConversacionId)
                .HasConstraintName("FK_chat_msg_conv");
        });

        modelBuilder.Entity<MovimientosStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movimien__3213E83FE3164B76");

            entity.ToTable("movimientos_stock", "ventas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(12, 3)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Fecha)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Referencia)
                .HasMaxLength(60)
                .HasColumnName("referencia");
            entity.Property(e => e.Tipo)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("tipo");

            entity.HasOne(d => d.Producto).WithMany(p => p.MovimientosStocks)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_mov_producto");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pagos__3213E83F8509E8C8");

            entity.ToTable("pagos", "ventas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fecha)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.Metodo)
                .HasMaxLength(20)
                .HasColumnName("metodo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Venta).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("FK_pagos_venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__producto__3213E83F1D73A39C");

            entity.ToTable("productos", "ventas");

            entity.HasIndex(e => e.Barcode, "UQ__producto__C16E36F8EFE1F94F").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__producto__DDDF4BE7B3E79C0C").IsUnique();

            entity.HasIndex(e => e.Barcode, "idx_productos_barcode");

            entity.HasIndex(e => e.Nombre, "idx_productos_nombre");

            entity.HasIndex(e => e.Sku, "idx_productos_sku");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Barcode)
                .HasMaxLength(64)
                .HasColumnName("barcode");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Costo)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("costo");
            entity.Property(e => e.CreadoEn)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EsPerecible).HasColumnName("es_perecible");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.Stock)
                .HasColumnType("decimal(12, 3)")
                .HasColumnName("stock");
            entity.Property(e => e.UnidadId).HasColumnName("unidad_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK_productos_categoria");

            entity.HasOne(d => d.Unidad).WithMany(p => p.Productos)
                .HasForeignKey(d => d.UnidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_productos_unidad");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83F5FA9F178");

            entity.ToTable("roles", "ventas");

            entity.HasIndex(e => e.Nombre, "UQ__roles__72AFBCC69F32C578").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Unidade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__unidades__3213E83F84A53FAC");

            entity.ToTable("unidades", "ventas");

            entity.HasIndex(e => e.Codigo, "UQ__unidades__40F9A206C23E9A3B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__usuarios__3213E83F98BBA531");

            entity.ToTable("usuarios", "ventas");

            entity.HasIndex(e => e.Username, "UQ__usuarios__F3DBC572D99E47C7").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.HashPassword)
                .HasMaxLength(200)
                .HasColumnName("hash_password");
            entity.Property(e => e.Username)
                .HasMaxLength(60)
                .HasColumnName("username");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EmpleadoId)
                .HasConstraintName("FK_usuarios_empleado");

            entity.HasMany(d => d.Rols).WithMany(p => p.Usuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuarioRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RolId")
                        .HasConstraintName("FK_usuario_roles_rol"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("UsuarioId")
                        .HasConstraintName("FK_usuario_roles_usuario"),
                    j =>
                    {
                        j.HasKey("UsuarioId", "RolId").HasName("PK__usuario___0224FCEB642BCC67");
                        j.ToTable("usuario_roles", "ventas");
                        j.IndexerProperty<long>("UsuarioId").HasColumnName("usuario_id");
                        j.IndexerProperty<int>("RolId").HasColumnName("rol_id");
                    });
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ventas__3213E83FE0860E63");

            entity.ToTable("ventas", "ventas", tb => tb.HasTrigger("trg_venta_anulada_reponer"));

            entity.HasIndex(e => e.Fecha, "idx_ventas_fecha");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(12)
                .HasDefaultValue("ACTIVA")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("total");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Venta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ventas_usuario");
        });

        modelBuilder.Entity<VentasDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ventas_d__3213E83F143DD752");

            entity.ToTable("ventas_detalle", "ventas", tb =>
                {
                    tb.HasTrigger("trg_detalle_delete_stock");
                    tb.HasTrigger("trg_detalle_insert_stock");
                });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(12, 3)")
                .HasColumnName("cantidad");
            entity.Property(e => e.Importe)
                .HasComputedColumnSql("(CONVERT([decimal](14,2),[cantidad]*[precio]))", true)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("importe");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Producto).WithMany(p => p.VentasDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_detalle_producto");

            entity.HasOne(d => d.Venta).WithMany(p => p.VentasDetalles)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("FK_detalle_venta");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
