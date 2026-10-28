using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class Plato
{
    [Display(Name = "Código")]
    public int CodPlato { get; set; }

    [Required(ErrorMessage = "El nombre del plato es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 10000, ErrorMessage = "El precio debe estar entre 0.01 y 10000.")]
    public decimal Precio { get; set; }

    public ICollection<PedidoPlato> PedidoPlatos { get; set; } = new List<PedidoPlato>();
}
