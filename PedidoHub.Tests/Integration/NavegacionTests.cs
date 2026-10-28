using System.Net;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Integration;

/// <summary>Humo (smoke) de la aplicación: todas las pantallas principales cargan sin errores.</summary>
public class NavegacionTests : PruebaIntegracion
{
    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Servicio")]
    [InlineData("/Calidad")]
    [InlineData("/Bitacora")]
    [InlineData("/Clientes")]
    [InlineData("/Clientes/Details/1")]
    [InlineData("/Clientes/Create")]
    [InlineData("/Mesas")]
    [InlineData("/Empleados")]
    [InlineData("/Platos")]
    [InlineData("/Reservas")]
    [InlineData("/Reservas/Create")]
    [InlineData("/Pedidos")]
    [InlineData("/Pedidos/Details/5")]
    [InlineData("/Pedidos/Create")]
    [InlineData("/Facturas")]
    [InlineData("/Facturas/Details/1")]
    [InlineData("/Reportes/Ventas")]
    [InlineData("/Reportes/PlatosMasVendidos")]
    public async Task Pantalla_CargaConExito(string url)
    {
        var respuesta = await Cliente.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("/Clientes/Details/9999")]
    [InlineData("/Pedidos/Details/9999")]
    [InlineData("/Facturas/Details/9999")]
    [InlineData("/Platos/Edit/9999")]
    public async Task RegistroInexistente_DevuelveNotFound(string url)
    {
        var respuesta = await Cliente.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Inicio_MuestraElPlanoDeMesasConLasCincoMesas()
    {
        var html = await LeerAsync("/Home/Servicio");

        for (var mesa = 1; mesa <= 5; mesa++)
        {
            Assert.Contains($"Mesa {mesa}", html);
        }
    }

    [Fact]
    public async Task ReporteVentas_ConFechasInvertidas_MuestraElError()
    {
        var html = await LeerAsync("/Reportes/Ventas?Desde=2026-10-20&Hasta=2026-10-01");

        Assert.Contains("La fecha final no puede ser anterior a la inicial.", html);
    }

    [Fact]
    public async Task ReporteVentas_MuestraTotalFacturadoDeLosDatosDeEjemplo()
    {
        var html = await LeerAsync("/Reportes/Ventas");

        Assert.Contains("603", html);
    }

    [Fact]
    public async Task Health_ReportaQueLaAplicacionYLaBaseDeDatosEstanVivas()
    {
        var respuesta = await Cliente.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("Healthy", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task RespuestasHtml_IncluyenCabecerasDeSeguridad()
    {
        var respuesta = await Cliente.GetAsync("/");

        Assert.True(respuesta.Headers.Contains("X-Content-Type-Options"));
        Assert.True(respuesta.Headers.Contains("X-Frame-Options"));
        Assert.True(respuesta.Headers.Contains("Referrer-Policy"));
    }

    [Fact]
    public async Task PostSinTokenAntiforgery_EsRechazado()
    {
        var respuesta = await Cliente.PostAsync("/Platos/Create",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["Nombre"] = "Hack", ["Precio"] = "10" }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
