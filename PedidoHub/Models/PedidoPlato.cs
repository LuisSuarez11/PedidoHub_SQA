using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoHub.Models;

// Tabla intermedia de la relación N:M "contiene" entre Pedido y Plato.
public class PedidoPlato
{
    public int IdPedido { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public int CodPlato { get; set; }
    public Plato Plato { get; set; } = null!;

    public int Cantidad { get; set; }

    [NotMapped]
    public decimal Subtotal => Cantidad * (Plato?.Precio ?? 0);
}
