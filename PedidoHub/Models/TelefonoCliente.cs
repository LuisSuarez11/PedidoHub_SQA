using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public class TelefonoCliente
{
    public int IdTelefono { get; set; }

    [Display(Name = "Número")]
    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número no puede superar los 20 caracteres.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Ingrese un teléfono válido (solo dígitos, espacios, + o -; mínimo 7).")]
    public string Numero { get; set; } = string.Empty;

    public int IdCliente { get; set; }
    public Cliente? Cliente { get; set; }
}
