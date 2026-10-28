using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class Factura
{
    [Display(Name = "N° de factura")]
    public int NumFact { get; set; }

    [Required(ErrorMessage = "El NIT es obligatorio.")]
    [StringLength(20, ErrorMessage = "El NIT no puede superar los 20 caracteres.")]
    [RegularExpression(@"^[0-9]{5,20}$", ErrorMessage = "El NIT debe contener solo dígitos (mínimo 5).")]
    public string NIT { get; set; } = string.Empty;

    [Display(Name = "Fecha de emisión")]
    public DateTime FechaEmision { get; set; }

    public int IdPedido { get; set; }
    public Pedido Pedido { get; set; } = null!;
}
