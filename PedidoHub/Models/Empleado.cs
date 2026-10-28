using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class Empleado
{
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "El nombre del empleado es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El cargo no puede superar los 50 caracteres.")]
    public string Cargo { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
