using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class Cliente
{
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<TelefonoCliente> Telefonos { get; set; } = new List<TelefonoCliente>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
