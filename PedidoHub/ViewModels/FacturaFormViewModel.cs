using System.ComponentModel.DataAnnotations;

namespace PedidoHub.ViewModels;

public class FacturaFormViewModel
{
    public int IdPedido { get; set; }

    [Required(ErrorMessage = "El NIT es obligatorio.")]
    [StringLength(20, ErrorMessage = "El NIT no puede superar los 20 caracteres.")]
    [RegularExpression(@"^[0-9]{5,20}$", ErrorMessage = "El NIT debe contener solo dígitos (mínimo 5).")]
    public string NIT { get; set; } = string.Empty;

    [Display(Name = "Fecha de emisión")]
    [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime FechaEmision { get; set; } = DateTime.Now;

    // Datos informativos del pedido a facturar (solo lectura).
    public string DescripcionPedido { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
