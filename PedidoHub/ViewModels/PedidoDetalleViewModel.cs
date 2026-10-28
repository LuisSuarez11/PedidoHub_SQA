using PedidoHub.Models;

namespace PedidoHub.ViewModels;

public class PedidoDetalleViewModel
{
    public Pedido Pedido { get; set; } = null!;
    public AgregarPlatoViewModel NuevoPlato { get; set; } = new();

    public bool PuedeModificarse =>
        Pedido.Estado == EstadoPedido.Abierto || Pedido.Estado == EstadoPedido.Servido;
}
