using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PedidoHub.Tests.Infra;

/// <summary>
/// SQLite no puede sumar columnas decimal. Para que las consultas con Sum() se ejecuten en las pruebas,
/// los decimales se almacenan como double solo en la base de datos de prueba; el modelo de la aplicación no cambia.
/// </summary>
public class PersonalizadorDecimalesSqlite : RelationalModelCustomizer
{
    public PersonalizadorDecimalesSqlite(ModelCustomizerDependencies dependencias) : base(dependencias)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        var propiedades = modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));
        foreach (var propiedad in propiedades)
        {
            propiedad.SetProviderClrType(typeof(double));
        }
    }
}
