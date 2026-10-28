using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Tests.Unit;

/// <summary>Pruebas unitarias de los atributos derivados (total del pedido, métricas de reportes).</summary>
public class CalculosDominioTests
{
    [Fact]
    public void PedidoPlato_Subtotal_EsCantidadPorPrecio()
    {
        var detalle = new PedidoPlato { Cantidad = 3, Plato = new Plato { Precio = 12.5m } };

        Assert.Equal(37.5m, detalle.Subtotal);
    }

    [Fact]
    public void PedidoPlato_SinPlatoCargado_TieneSubtotalCero()
    {
        var detalle = new PedidoPlato { Cantidad = 3 };

        Assert.Equal(0m, detalle.Subtotal);
    }

    [Fact]
    public void Pedido_Total_SumaLosSubtotalesDeSusPlatos()
    {
        var pedido = new Pedido();
        pedido.Detalles.Add(new PedidoPlato { Cantidad = 2, Plato = new Plato { Precio = 55m } });
        pedido.Detalles.Add(new PedidoPlato { Cantidad = 2, Plato = new Plato { Precio = 10m } });

        Assert.Equal(130m, pedido.Total);
    }

    [Fact]
    public void Pedido_SinPlatos_TieneTotalCero()
    {
        Assert.Equal(0m, new Pedido().Total);
    }

    [Fact]
    public void ReporteVentas_SinFilas_NoDivideEntreCero()
    {
        var reporte = new ReporteVentasViewModel();

        Assert.Equal(0, reporte.CantidadFacturas);
        Assert.Equal(0m, reporte.TicketPromedio);
    }

    [Fact]
    public void ReporteVentas_CalculaTotalYTicketPromedio()
    {
        var reporte = new ReporteVentasViewModel
        {
            Filas =
            {
                new VentaFila(1, DateTime.Today, 1, 1, "Laura", "12345", 100m),
                new VentaFila(2, DateTime.Today, 2, 2, "Pedro", "67890", 50m)
            }
        };

        Assert.Equal(2, reporte.CantidadFacturas);
        Assert.Equal(150m, reporte.TotalFacturado);
        Assert.Equal(75m, reporte.TicketPromedio);
    }
}
