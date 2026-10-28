using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.Services;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class HomeController : Controller
{
    private const int DiasResumen = 30;
    private const int PlatosEnRanking = 3;

    private readonly PedidoHubDbContext _context;
    private readonly ReporteService _reportes;

    public HomeController(PedidoHubDbContext context, ReporteService reportes)
    {
        _context = context;
        _reportes = reportes;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        var pedidos = await _context.Pedidos.AsNoTracking()
            .Where(p => p.Factura == null && (p.Estado == EstadoPedido.Abierto || p.Estado == EstadoPedido.Servido))
            .OrderBy(p => p.FechaHora)
            .Select(p => new
            {
                p.IdPedido,
                p.NumMesa,
                Empleado = p.Empleado.Nombre,
                p.FechaHora,
                Total = p.Detalles.Sum(d => d.Cantidad * d.Plato.Precio)
            })
            .ToListAsync();
        var porCobrar = pedidos
            .Select(p => new PedidoAbiertoFila(p.IdPedido, p.NumMesa, p.Empleado, p.FechaHora, p.Total))
            .ToList();

        var reservas = await _context.Reservas.AsNoTracking()
            .Where(r => r.FechaHora >= hoy &&
                        (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada))
            .OrderBy(r => r.FechaHora)
            .Select(r => new ReservaProximaFila(r.FechaHora, r.Cliente.Nombre, r.NumMesa, r.CantPersonas, r.Estado))
            .ToListAsync();

        var mesas = await _context.Mesas.AsNoTracking().OrderBy(m => m.NumMesa).ToListAsync();
        var plano = mesas.Select(m =>
        {
            var pedido = porCobrar.FirstOrDefault(p => p.NumMesa == m.NumMesa);
            var reserva = reservas.FirstOrDefault(r => r.NumMesa == m.NumMesa);
            var estado = pedido is not null ? EstadoMesa.Ocupada
                : reserva is not null && reserva.FechaHora < manana ? EstadoMesa.ReservadaHoy
                : EstadoMesa.Libre;
            return new MesaFila(m.NumMesa, m.Capacidad, estado, pedido, reserva);
        }).ToList();

        var ventas = await _reportes.ObtenerVentasAsync(hoy.AddDays(-DiasResumen), hoy, null);
        var platos = await _reportes.ObtenerPlatosMasVendidosAsync(hoy.AddDays(-DiasResumen), hoy, PlatosEnRanking);

        return View(new InicioViewModel
        {
            Mesas = plano,
            PorCobrar = porCobrar,
            ProximasReservas = reservas.Take(4).ToList(),
            ReservasPorAtender = reservas.Count,
            PlatosTop = platos,
            VentasTotal = ventas.Sum(v => v.Total),
            FacturasEmitidas = ventas.Count
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
