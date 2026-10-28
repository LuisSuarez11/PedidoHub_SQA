using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class PlatoConfiguration : IEntityTypeConfiguration<Plato>
{
    public void Configure(EntityTypeBuilder<Plato> builder)
    {
        builder.ToTable("Platos", table =>
            table.HasCheckConstraint("CK_Platos_Precio", "[Precio] > 0"));
        builder.HasKey(p => p.CodPlato);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Precio).HasColumnType("decimal(10,2)");
        builder.HasIndex(p => p.Nombre).IsUnique();

        builder.HasData(
            new Plato { CodPlato = 1, Nombre = "Pique Macho", Precio = 55m },
            new Plato { CodPlato = 2, Nombre = "Silpancho", Precio = 40m },
            new Plato { CodPlato = 3, Nombre = "Sopa de Maní", Precio = 25m },
            new Plato { CodPlato = 4, Nombre = "Salteña", Precio = 12m },
            new Plato { CodPlato = 5, Nombre = "Refresco", Precio = 10m },
            new Plato { CodPlato = 6, Nombre = "Flan", Precio = 18m });
    }
}
