using PedidoHub.Services;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Unit;

/// <summary>
/// Pruebas del servicio de reportes contra los datos de ejemplo (4 facturas por 603.00 en total;
/// el pedido 5 está abierto, por lo que no cuenta como venta).
/// </summary>
public class ReporteServiceTests : IDisposable
{
    private readonly BaseDatosDePrueba _bd = new();
    private readonly ReporteService _servicio;

    public ReporteServiceTests()
    {
        _servicio = new ReporteService(_bd.NuevoContexto());
    }

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task Ventas_SinFiltros_DevuelveTodasLasFacturasOrdenadasPorFecha()
    {
        var filas = await _servicio.ObtenerVentasAsync(null, null, null);

        Assert.Equal(new[] { 1, 2, 3, 4 }, filas.Select(f => f.NumFact));
        Assert.Equal(603m, filas.Sum(f => f.Total));
    }

    [Fact]
    public async Task Ventas_CalculaElTotalDeCadaFacturaConCantidadPorPrecio()
    {
        var filas = await _servicio.ObtenerVentasAsync(null, null, null);

        Assert.Equal(new[] { 130m, 186m, 172m, 115m }, filas.Select(f => f.Total));
    }

    [Fact]
    public async Task Ventas_FiltradasPorEmpleado_SoloIncluyeSusPedidos()
    {
        var filas = await _servicio.ObtenerVentasAsync(null, null, idEmpleado: 1);

        Assert.Equal(new[] { 1, 3 }, filas.Select(f => f.NumFact));
        Assert.All(filas, f => Assert.Equal("Laura Gómez", f.Empleado));
    }

    [Fact]
    public async Task Ventas_ElLimiteSuperiorIncluyeTodoElDiaFinal()
    {
        // La factura 2 se emite el 10/10 a las 15:10; "hasta 10/10" debe incluirla.
        var filas = await _servicio.ObtenerVentasAsync(new DateTime(2026, 10, 10), new DateTime(2026, 10, 10), null);

        Assert.Equal(new[] { 2 }, filas.Select(f => f.NumFact));
    }

    [Fact]
    public async Task Ventas_RangoSinFacturas_DevuelveListaVacia()
    {
        var filas = await _servicio.ObtenerVentasAsync(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31), null);

        Assert.Empty(filas);
    }

    [Fact]
    public async Task PlatosMasVendidos_OrdenaPorUnidadesYDesempataPorNombre()
    {
        var filas = await _servicio.ObtenerPlatosMasVendidosAsync(null, null, top: 10);

        Assert.Equal("Refresco", filas[0].Plato);
        Assert.Equal(7, filas[0].Unidades);
        Assert.Equal(new[] { "Flan", "Silpancho", "Sopa de Maní" }, filas.Skip(1).Take(3).Select(f => f.Plato));
    }

    [Fact]
    public async Task PlatosMasVendidos_IgnoraLosPedidosSinFacturar()
    {
        var filas = await _servicio.ObtenerPlatosMasVendidosAsync(null, null, top: 10);

        // El pedido 5 (abierto) tiene 6 salteñas; solo se cuentan las 3 de pedidos facturados.
        Assert.Equal(3, filas.Single(f => f.Plato == "Salteña").Unidades);
    }

    [Fact]
    public async Task PlatosMasVendidos_RespetaElLimiteTop()
    {
        var filas = await _servicio.ObtenerPlatosMasVendidosAsync(null, null, top: 2);

        Assert.Equal(2, filas.Count);
    }

    [Fact]
    public async Task PlatosMasVendidos_CalculaMontoComoPrecioPorUnidades()
    {
        var filas = await _servicio.ObtenerPlatosMasVendidosAsync(null, null, top: 10);

        var refresco = filas.Single(f => f.Plato == "Refresco");
        Assert.Equal(70m, refresco.Monto);
    }
}
