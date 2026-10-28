using System.ComponentModel.DataAnnotations;

namespace PedidoHub.ViewModels;

// Alta de cliente: el DER exige que un cliente tenga al menos un teléfono (cardinalidad (1,n) en "posee").
public class ClienteFormViewModel
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "Ingrese al menos un teléfono de contacto.")]
    [StringLength(20, ErrorMessage = "El número no puede superar los 20 caracteres.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Ingrese un teléfono válido (solo dígitos, espacios, + o -; mínimo 7).")]
    public string Telefono { get; set; } = string.Empty;
}
