using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.IdUsuario);
        builder.HasIndex(u => u.NombreUsuario).IsUnique();
        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(30);
        builder.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Rol).IsRequired().HasMaxLength(20);
        builder.Property(u => u.ClaveHash).IsRequired().HasMaxLength(200);
    }
}

public class BitacoraConfiguration : IEntityTypeConfiguration<Bitacora>
{
    public void Configure(EntityTypeBuilder<Bitacora> builder)
    {
        builder.ToTable("Bitacoras");
        builder.HasKey(b => b.IdBitacora);
        builder.HasIndex(b => b.FechaHora);
        builder.Property(b => b.Usuario).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Accion).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Entidad).IsRequired().HasMaxLength(40);
        builder.Property(b => b.Registro).IsRequired().HasMaxLength(60);
        builder.Property(b => b.Detalle).IsRequired().HasMaxLength(500);
    }
}