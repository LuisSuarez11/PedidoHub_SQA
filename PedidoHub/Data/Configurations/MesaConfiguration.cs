using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("Mesas", table =>
            table.HasCheckConstraint("CK_Mesas_Capacidad", "[Capacidad] BETWEEN 1 AND 20"));
        builder.HasKey(m => m.NumMesa);
        builder.Property(m => m.Capacidad).IsRequired();

        builder.HasData(
            new Mesa { NumMesa = 1, Capacidad = 2 },
            new Mesa { NumMesa = 2, Capacidad = 4 },
            new Mesa { NumMesa = 3, Capacidad = 4 },
            new Mesa { NumMesa = 4, Capacidad = 6 },
            new Mesa { NumMesa = 5, Capacidad = 8 });
    }
}
