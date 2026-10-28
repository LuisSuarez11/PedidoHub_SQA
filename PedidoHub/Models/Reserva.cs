using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class Reserva
{
    public int IdReserva { get; set; }

    [Display(Name = "Fecha y hora")]
    public DateTime FechaHora { get; set; }

    [Display(Name = "Personas")]
    public int CantPersonas { get; set; }

    public EstadoReserva Estado { get; set; }

    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int NumMesa { get; set; }
    public Mesa Mesa { get; set; } = null!;
}
