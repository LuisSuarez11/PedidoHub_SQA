using Microsoft.AspNetCore.Mvc.Rendering;
using PedidoHub.Models;

namespace PedidoHub.ViewModels;

// Contenido del cajón lateral que se abre al tocar una mesa en el plano.
public class MesaPanelViewModel
{
    public Mesa Mesa { get; set; } = null!;

    // Pedido sin facturar de la mesa, si lo hay.
    public Pedido? Pedido { get; set; }

    public List<Reserva> ProximasReservas { get; set; } = new();
    public IEnumerable<SelectListItem> Empleados { get; set; } = new List<SelectListItem>();
}
