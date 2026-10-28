using System.ComponentModel.DataAnnotations;

namespace PedidoHub.ViewModels;

public class IngresarViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "Ingrese su usuario.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    public string Clave { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}