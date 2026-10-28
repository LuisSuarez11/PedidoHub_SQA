using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PedidoHub.Models;

namespace PedidoHub.ViewModels;

public class ReservaFormViewModel
{
    public int IdReserva { get; set; }

    [Display(Name = "Fecha y hora")]
    [Required(ErrorMessage = "La fecha y hora de la reserva son obligatorias.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime FechaHora { get; set; } = DateTime.Today.AddDays(1).AddHours(20);

    [Display(Name = "Cantidad de personas")]
    [Required(ErrorMessage = "Indique la cantidad de personas.")]
    [Range(1, 50, ErrorMessage = "La cantidad de personas debe estar entre 1 y 50.")]
    public int CantPersonas { get; set; } = 2;

    [Required(ErrorMessage = "Seleccione el estado.")]
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

    [Display(Name = "Cliente")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int IdCliente { get; set; }

    [Display(Name = "Mesa")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una mesa.")]
    public int NumMesa { get; set; }

    public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Mesas { get; set; } = new List<SelectListItem>();
}
