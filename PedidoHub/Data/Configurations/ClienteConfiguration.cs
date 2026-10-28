using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.IdCliente);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);

        builder.HasData(
            new Cliente { IdCliente = 1, Nombre = "Ana Torres" },
            new Cliente { IdCliente = 2, Nombre = "Bruno Díaz" },
            new Cliente { IdCliente = 3, Nombre = "Carla Méndez" },
            new Cliente { IdCliente = 4, Nombre = "Diego Ruiz" });
    }
}
