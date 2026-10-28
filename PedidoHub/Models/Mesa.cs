using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class Mesa
{
    [Display(Name = "N° de mesa")]
    public int NumMesa { get; set; }

    [Required(ErrorMessage = "La capacidad es obligatoria.")]
    [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20 personas.")]
    public int Capacidad { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
