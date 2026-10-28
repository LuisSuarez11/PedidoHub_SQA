using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PedidoHub.ViewModels;

public class AgregarPlatoViewModel
{
    public int IdPedido { get; set; }

    [Display(Name = "Plato")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un plato.")]
    public int CodPlato { get; set; }

    [Required(ErrorMessage = "Indique la cantidad.")]
    [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
    public int Cantidad { get; set; } = 1;

    public IEnumerable<SelectListItem> Platos { get; set; } = new List<SelectListItem>();
}
