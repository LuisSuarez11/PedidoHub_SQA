using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class TelefonoClienteConfiguration : IEntityTypeConfiguration<TelefonoCliente>
{
    public void Configure(EntityTypeBuilder<TelefonoCliente> builder)
    {
        builder.ToTable("TelefonosCliente");
        builder.HasKey(t => t.IdTelefono);
        builder.Property(t => t.Numero).IsRequired().HasMaxLength(20);

        // Un cliente posee varios teléfonos (1:N). Al eliminar el cliente se eliminan sus teléfonos.
        builder.HasOne(t => t.Cliente)
            .WithMany(c => c.Telefonos)
            .HasForeignKey(t => t.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new TelefonoCliente { IdTelefono = 1, Numero = "71234567", IdCliente = 1 },
            new TelefonoCliente { IdTelefono = 2, Numero = "69871122", IdCliente = 1 },
            new TelefonoCliente { IdTelefono = 3, Numero = "70001234", IdCliente = 2 },
            new TelefonoCliente { IdTelefono = 4, Numero = "76543210", IdCliente = 3 },
            new TelefonoCliente { IdTelefono = 5, Numero = "78889999", IdCliente = 4 });
    }
}
