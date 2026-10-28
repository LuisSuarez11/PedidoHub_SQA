using System.Net;
using PedidoHub.Models;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Integration;

/// <summary>Exportación de reportes a CSV (HU-17).</summary>
public class ReportesCsvTests : PruebaIntegracion
{
    [Fact]
    public async Task Dado_el_reporte_de_ventas_Cuando_se_exporta_Entonces_devuelve_un_CSV_con_las_cuatro_facturas()
    {
        var respuesta = await Cliente.GetAsync("/Reportes/VentasCsv");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.StartsWith("text/csv", respuesta.Content.Headers.ContentType!.ToString());
        Assert.Equal("ventas.csv", respuesta.Content.Headers.ContentDisposition!.FileName);
        var lineas = (await respuesta.Content.ReadAsStringAsync()).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, lineas.Length);
        Assert.StartsWith("Factura,Fecha emision", lineas[0].TrimStart('\uFEFF'));
        Assert.Contains("130.00", lineas[1]);
    }

    [Fact]
    public async Task Dado_un_filtro_por_empleado_Cuando_se_exporta_Entonces_solo_incluye_sus_facturas()
    {
        var csv = await Cliente.GetStringAsync("/Reportes/VentasCsv?idEmpleado=2");

        Assert.Contains("Pedro Salas", csv);
        Assert.DoesNotContain("Laura", csv);
    }

    [Fact]
    public async Task Dadas_fechas_invertidas_Cuando_se_exporta_Entonces_responde_BadRequest()
    {
        var respuesta = await Cliente.GetAsync("/Reportes/VentasCsv?desde=2026-10-20&hasta=2026-10-01");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Dado_el_reporte_de_platos_Cuando_se_exporta_con_top_2_Entonces_devuelve_dos_filas()
    {
        var csv = await Cliente.GetStringAsync("/Reportes/PlatosCsv?top=2");

        Assert.Equal(3, csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("Refresco", csv);
    }

    [Fact]
    public async Task Dado_un_nombre_que_empieza_con_formula_Cuando_se_exporta_Entonces_se_neutraliza_la_inyeccion_en_Excel()
    {
        await ConBaseDeDatosAsync(async bd =>
        {
            var empleado = new Empleado { Nombre = "=HYPERLINK(\"http://x\")", Cargo = "Mesero" };
            var pedido = new Pedido { FechaHora = new DateTime(2026, 10, 26, 12, 0, 0), Estado = EstadoPedido.Facturado, NumMesa = 1, Empleado = empleado };
            pedido.Detalles.Add(new PedidoPlato { CodPlato = 1, Cantidad = 1 });
            pedido.Factura = new Factura { NIT = "1234567", FechaEmision = new DateTime(2026, 10, 26, 13, 0, 0) };
            bd.Add(pedido);
            return await bd.SaveChangesAsync();
        });

        var csv = await Cliente.GetStringAsync("/Reportes/VentasCsv?desde=2026-10-26&hasta=2026-10-26");

        Assert.DoesNotContain(",=HYPERLINK", csv);
        Assert.Contains("\"\u0027=HYPERLINK", csv);
    }

    [Fact]
    public async Task Dado_el_reporte_en_pantalla_Cuando_se_abre_Entonces_ofrece_el_boton_Exportar_CSV()
    {
        Assert.Contains("Exportar CSV", await LeerAsync("/Reportes/Ventas"));
        Assert.Contains("Exportar CSV", await LeerAsync("/Reportes/PlatosMasVendidos"));
    }
}