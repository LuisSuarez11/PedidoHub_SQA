using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.ViewModels;

namespace PedidoHub.Services;

public class ReporteService
{
    private readonly PedidoHubDbContext _context;

    public ReporteService(PedidoHubDbContext context)
    {
        _context = context;
    }

    // Reporte 1: facturas emitidas en un período, opcionalmente de un empleado.
    public async Task<List<VentaFila>> ObtenerVentasAsync(DateTime? desde, DateTime? hasta, int? idEmpleado)
    {
        var consulta = _context.Facturas.AsNoTracking().AsQueryable();

        if (desde.HasValue)
        {
            consulta = consulta.Where(f => f.FechaEmision >= desde.Value.Date);
        }
        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(f => f.FechaEmision < limite);
        }
        if (idEmpleado.HasValue)
        {
            consulta = consulta.Where(f => f.Pedido.IdEmpleado == idEmpleado.Value);
        }

        return await consulta
            .OrderBy(f => f.FechaEmision)
            .Select(f => new VentaFila(
                f.NumFact,
                f.FechaEmision,
                f.IdPedido,
                f.Pedido.NumMesa,
                f.Pedido.Empleado.Nombre,
                f.NIT,
                f.Pedido.Detalles.Sum(d => d.Cantidad * d.Plato.Precio)))
            .ToListAsync();
    }

    // Reporte 2: platos más vendidos por fecha de emisión de la factura, igual que Ventas.
    public async Task<List<PlatoVendidoFila>> ObtenerPlatosMasVendidosAsync(DateTime? desde, DateTime? hasta, int top)
    {
        var consulta = _context.PedidoPlatos.AsNoTracking()
            .Where(d => d.Pedido.Factura != null);

        if (desde.HasValue)
        {
            consulta = consulta.Where(d => d.Pedido.Factura!.FechaEmision >= desde.Value.Date);
        }
        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(d => d.Pedido.Factura!.FechaEmision < limite);
        }

        var filas = await consulta
            .GroupBy(d => new { d.CodPlato, d.Plato.Nombre, d.Plato.Precio })
            .Select(g => new { g.Key.CodPlato, g.Key.Nombre, g.Key.Precio, Unidades = g.Sum(d => d.Cantidad) })
            .OrderByDescending(f => f.Unidades)
            .ThenBy(f => f.Nombre)
            .Take(top)
            .ToListAsync();

        return filas
            .Select(f => new PlatoVendidoFila(f.CodPlato, f.Nombre, f.Precio, f.Unidades, f.Precio * f.Unidades))
            .ToList();
    }
}
