using System.ComponentModel.DataAnnotations;

namespace PedidoHub.Models;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Mesero = "Mesero";
    public const string Cajero = "Cajero";

    public static readonly string[] Todos = { Administrador, Mesero, Cajero };
}

public class Usuario
{
    public int IdUsuario { get; set; }

    [Required, StringLength(30)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Rol { get; set; } = Roles.Mesero;

    [Required, StringLength(200)]
    public string ClaveHash { get; set; } = string.Empty;
}