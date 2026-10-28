using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidoHub.Models;

public class Pedido
{
    public int IdPedido { get; set; }

    [Display(Name = "Fecha y hora")]
    public DateTime FechaHora { get; set; }

    public EstadoPedido Estado { get; set; }

    public int NumMesa { get; set; }
    public Mesa Mesa { get; set; } = null!;

    public int IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Factura? Factura { get; set; }
    public ICollection<PedidoPlato> Detalles { get; set; } = new List<PedidoPlato>();

    // Atributo derivado del DER (Total_Calc): se calcula, no se guarda.
    [NotMapped]
    public decimal Total => Detalles.Sum(d => d.Subtotal);
}
