using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PedidoHub.Models;

namespace PedidoHub.Data;

public class PedidoHubDbContext : DbContext
{
    private readonly IHttpContextAccessor? _accesor;
    private bool _registrandoBitacora;

    public PedidoHubDbContext(DbContextOptions<PedidoHubDbContext> options, IHttpContextAccessor? accesor = null) : base(options)
    {
        _accesor = accesor;
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
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Bitacora> Bitacoras => Set<Bitacora>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidoHubDbContext).Assembly);
    }

    // Bitácora de auditoría: cada alta, cambio o baja queda registrada con el usuario que la hizo.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (_registrandoBitacora)
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        var cambios = ChangeTracker.Entries()
            .Where(e => e.Entity is not Bitacora and not Usuario &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => (Entrada: e, Estado: e.State, Detalle: DescribirCambios(e)))
            .ToList();

        var resultado = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (cambios.Count == 0)
        {
            return resultado;
        }

        var usuario = _accesor?.HttpContext?.User.Identity?.Name ?? "sistema";
        foreach (var (entrada, estado, detalle) in cambios)
        {
            Bitacoras.Add(new Bitacora
            {
                FechaHora = DateTime.Now,
                Usuario = usuario,
                Accion = estado switch { EntityState.Added => "Creó", EntityState.Modified => "Modificó", _ => "Eliminó" },
                Entidad = entrada.Metadata.ClrType.Name,
                Registro = ClaveDe(entrada),
                Detalle = detalle.Length > 500 ? detalle[..497] + "..." : detalle
            });
        }

        _registrandoBitacora = true;
        try
        {
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally
        {
            _registrandoBitacora = false;
        }
        return resultado;
    }

    private static string ClaveDe(EntityEntry entrada) =>
        string.Join("-", entrada.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue));

    private static string DescribirCambios(EntityEntry entrada)
    {
        if (entrada.State != EntityState.Modified)
        {
            return entrada.State == EntityState.Added ? "Registro nuevo" : "Registro eliminado";
        }

        var cambios = entrada.Properties
            .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
            .Select(p => $"{p.Metadata.Name}: {p.OriginalValue} → {p.CurrentValue}");
        return string.Join("; ", cambios);
    }
}