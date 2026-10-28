using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reservas", table =>
            table.HasCheckConstraint("CK_Reservas_CantPersonas", "[CantPersonas] > 0"));
        builder.HasKey(r => r.IdReserva);
        builder.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);

        // Un cliente realiza muchas reservas (1:N).
        builder.HasOne(r => r.Cliente)
            .WithMany(c => c.Reservas)
            .HasForeignKey(r => r.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        // Una mesa tiene muchas reservas asignadas (1:N).
        builder.HasOne(r => r.Mesa)
            .WithMany(m => m.Reservas)
            .HasForeignKey(r => r.NumMesa)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Reserva { IdReserva = 1, FechaHora = new DateTime(2026, 10, 2, 20, 0, 0), CantPersonas = 2, Estado = EstadoReserva.Completada, IdCliente = 1, NumMesa = 1 },
            new Reserva { IdReserva = 2, FechaHora = new DateTime(2026, 10, 9, 13, 0, 0), CantPersonas = 4, Estado = EstadoReserva.Completada, IdCliente = 2, NumMesa = 2 },
            new Reserva { IdReserva = 3, FechaHora = new DateTime(2026, 10, 30, 21, 0, 0), CantPersonas = 6, Estado = EstadoReserva.Confirmada, IdCliente = 3, NumMesa = 4 },
            new Reserva { IdReserva = 4, FechaHora = new DateTime(2026, 11, 2, 12, 30, 0), CantPersonas = 3, Estado = EstadoReserva.Pendiente, IdCliente = 4, NumMesa = 3 });
    }
}
