using Microsoft.EntityFrameworkCore;
using PedidoHub.Models;

namespace PedidoHub.Data;

public class PedidoHubDbContext : DbContext
{
    public PedidoHubDbContext(DbContextOptions<PedidoHubDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<TelefonoCliente> Telefonos => Set<TelefonoCliente>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Plato> Platos => Set<Plato>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoPlato> PedidoPlatos => Set<PedidoPlato>();
    public DbSet<Factura> Facturas => Set<Factura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidoHubDbContext).Assembly);
    }
}
