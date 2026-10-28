using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas");
        builder.HasKey(f => f.NumFact);
        builder.Property(f => f.NIT).IsRequired().HasMaxLength(20);

        // Un pedido genera exactamente una factura (1:1): el índice único garantiza la unicidad.
        builder.HasOne(f => f.Pedido)
            .WithOne(p => p.Factura)
            .HasForeignKey<Factura>(f => f.IdPedido)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(f => f.IdPedido).IsUnique();

        builder.HasData(
            new Factura { NumFact = 1, NIT = "1020304011", FechaEmision = new DateTime(2026, 10, 3, 14, 30, 0), IdPedido = 1 },
            new Factura { NumFact = 2, NIT = "4455667788", FechaEmision = new DateTime(2026, 10, 10, 15, 10, 0), IdPedido = 2 },
            new Factura { NumFact = 3, NIT = "9988776655", FechaEmision = new DateTime(2026, 10, 17, 22, 0, 0), IdPedido = 3 },
            new Factura { NumFact = 4, NIT = "1020304011", FechaEmision = new DateTime(2026, 10, 24, 22, 15, 0), IdPedido = 4 });
    }
}
