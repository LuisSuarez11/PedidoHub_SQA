using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PedidoHub.Data;

namespace PedidoHub.Tests.Infra;

/// <summary>
/// Base de datos SQLite en memoria con el esquema y los datos de ejemplo del modelo real.
/// Cada instancia es una base nueva y aislada, por lo que las pruebas no se afectan entre sí.
/// </summary>
public sealed class BaseDatosDePrueba : IDisposable
{
    private readonly SqliteConnection _conexion;

    public BaseDatosDePrueba()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        using var contexto = NuevoContexto();
        contexto.Database.EnsureCreated();
    }

    public PedidoHubDbContext NuevoContexto() =>
        new(new DbContextOptionsBuilder<PedidoHubDbContext>()
            .UseSqlite(_conexion)
            .ReplaceService<IModelCustomizer, PersonalizadorDecimalesSqlite>()
            .Options);

    public void Dispose() => _conexion.Dispose();
}
