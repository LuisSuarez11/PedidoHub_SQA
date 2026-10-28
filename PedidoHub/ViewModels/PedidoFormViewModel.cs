using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PedidoHub.Models;

namespace PedidoHub.ViewModels;

public class PedidoFormViewModel
{
    public int IdPedido { get; set; }

    [Display(Name = "Fecha y hora")]
    [Required(ErrorMessage = "La fecha y hora del pedido son obligatorias.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime FechaHora { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Seleccione el estado.")]
    public EstadoPedido Estado { get; set; } = EstadoPedido.Abierto;

    [Display(Name = "Mesa")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una mesa.")]
    public int NumMesa { get; set; }

    [Display(Name = "Empleado")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el empleado que atiende.")]
    public int IdEmpleado { get; set; }

    public IEnumerable<SelectListItem> Mesas { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Empleados { get; set; } = new List<SelectListItem>();
}
