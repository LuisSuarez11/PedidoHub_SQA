using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoHub.Models;

namespace PedidoHub.Data.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");
        builder.HasKey(e => e.IdEmpleado);
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Cargo).IsRequired().HasMaxLength(50);

        builder.HasData(
            new Empleado { IdEmpleado = 1, Nombre = "Laura Gómez", Cargo = "Mesera" },
            new Empleado { IdEmpleado = 2, Nombre = "Pedro Salas", Cargo = "Mesero" },
            new Empleado { IdEmpleado = 3, Nombre = "Marta Rojas", Cargo = "Cajera" });
    }
}
