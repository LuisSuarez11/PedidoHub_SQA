using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PedidoHub.Data;

namespace PedidoHub.Tests.Infra;

/// <summary>Levanta la aplicación completa en memoria usando SQLite en lugar de SQL Server.</summary>
public class PedidoHubFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(servicios =>
        {
            servicios.RemoveAll<DbContextOptions<PedidoHubDbContext>>();

            _conexion.Open();
            servicios.AddDbContext<PedidoHubDbContext>(o => o.UseSqlite(_conexion)
                .ReplaceService<IModelCustomizer, PersonalizadorDecimalesSqlite>());

            using var proveedor = servicios.BuildServiceProvider();
            using var alcance = proveedor.CreateScope();
            alcance.ServiceProvider.GetRequiredService<PedidoHubDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _conexion.Dispose();
        }
    }
}
