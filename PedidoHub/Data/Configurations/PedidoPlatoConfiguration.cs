using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class PedidoPlatoConfiguration : IEntityTypeConfiguration<PedidoPlato>
{
    public void Configure(EntityTypeBuilder<PedidoPlato> builder)
    {
        builder.ToTable("PedidoPlatos", table =>
            table.HasCheckConstraint("CK_PedidoPlatos_Cantidad", "[Cantidad] > 0"));
        builder.HasKey(pp => new { pp.IdPedido, pp.CodPlato });
        builder.Ignore(pp => pp.Subtotal);

        // Relación N:M "contiene": un pedido contiene muchos platos y un plato está en muchos pedidos.
        builder.HasOne(pp => pp.Pedido)
            .WithMany(p => p.Detalles)
            .HasForeignKey(pp => pp.IdPedido)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Plato)
            .WithMany(p => p.PedidoPlatos)
            .HasForeignKey(pp => pp.CodPlato)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new PedidoPlato { IdPedido = 1, CodPlato = 1, Cantidad = 2 },
            new PedidoPlato { IdPedido = 1, CodPlato = 5, Cantidad = 2 },
            new PedidoPlato { IdPedido = 2, CodPlato = 2, Cantidad = 3 },
            new PedidoPlato { IdPedido = 2, CodPlato = 4, Cantidad = 3 },
            new PedidoPlato { IdPedido = 2, CodPlato = 5, Cantidad = 3 },
            new PedidoPlato { IdPedido = 3, CodPlato = 3, Cantidad = 4 },
            new PedidoPlato { IdPedido = 3, CodPlato = 6, Cantidad = 4 },
            new PedidoPlato { IdPedido = 4, CodPlato = 1, Cantidad = 1 },
            new PedidoPlato { IdPedido = 4, CodPlato = 2, Cantidad = 1 },
            new PedidoPlato { IdPedido = 4, CodPlato = 5, Cantidad = 2 },
            new PedidoPlato { IdPedido = 5, CodPlato = 4, Cantidad = 6 },
            new PedidoPlato { IdPedido = 5, CodPlato = 5, Cantidad = 4 });
    }
}
