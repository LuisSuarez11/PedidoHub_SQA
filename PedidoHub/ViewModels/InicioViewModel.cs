using PedidoHub.Models;

namespace PedidoHub.ViewModels;

public enum EstadoMesa
{
    Libre,
    Ocupada,
    ReservadaHoy
}

public record PedidoAbiertoFila(int IdPedido, int NumMesa, string Empleado, DateTime FechaHora, decimal Total);

public record ReservaProximaFila(DateTime FechaHora, string Cliente, int NumMesa, int CantPersonas, EstadoReserva Estado);

public record MesaFila(
    int NumMesa,
    int Capacidad,
    EstadoMesa Estado,
    PedidoAbiertoFila? Pedido,
    ReservaProximaFila? ProximaReserva);

public class InicioViewModel
{
    public List<MesaFila> Mesas { get; set; } = new();

    // Pedidos sin facturar (abiertos o servidos), el más antiguo primero.
    public List<PedidoAbiertoFila> PorCobrar { get; set; } = new();

    public List<ReservaProximaFila> ProximasReservas { get; set; } = new();
    public int ReservasPorAtender { get; set; }

    public List<PlatoVendidoFila> PlatosTop { get; set; } = new();

    // Ventas de los últimos 30 días.
    public decimal VentasTotal { get; set; }
    public int FacturasEmitidas { get; set; }
    public decimal TicketPromedio => FacturasEmitidas == 0 ? 0 : VentasTotal / FacturasEmitidas;

    public int MesasOcupadas => Mesas.Count(m => m.Estado == EstadoMesa.Ocupada);
    public decimal TotalPorCobrar => PorCobrar.Sum(p => p.Total);
}
