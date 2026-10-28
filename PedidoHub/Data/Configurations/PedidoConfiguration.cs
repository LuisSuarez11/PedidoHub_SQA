using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.IdPedido);
        builder.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(p => p.Total);

        // Una mesa registra muchos pedidos (1:N).
        builder.HasOne(p => p.Mesa)
            .WithMany(m => m.Pedidos)
            .HasForeignKey(p => p.NumMesa)
            .OnDelete(DeleteBehavior.Restrict);

        // Un empleado atiende muchos pedidos (1:N).
        builder.HasOne(p => p.Empleado)
            .WithMany(e => e.Pedidos)
            .HasForeignKey(p => p.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Pedido { IdPedido = 1, FechaHora = new DateTime(2026, 10, 3, 13, 15, 0), Estado = EstadoPedido.Facturado, NumMesa = 1, IdEmpleado = 1 },
            new Pedido { IdPedido = 2, FechaHora = new DateTime(2026, 10, 10, 14, 0, 0), Estado = EstadoPedido.Facturado, NumMesa = 2, IdEmpleado = 2 },
            new Pedido { IdPedido = 3, FechaHora = new DateTime(2026, 10, 17, 20, 30, 0), Estado = EstadoPedido.Facturado, NumMesa = 4, IdEmpleado = 1 },
            new Pedido { IdPedido = 4, FechaHora = new DateTime(2026, 10, 24, 21, 0, 0), Estado = EstadoPedido.Facturado, NumMesa = 3, IdEmpleado = 2 },
            new Pedido { IdPedido = 5, FechaHora = new DateTime(2026, 10, 25, 12, 45, 0), Estado = EstadoPedido.Abierto, NumMesa = 5, IdEmpleado = 1 });
    }
}
