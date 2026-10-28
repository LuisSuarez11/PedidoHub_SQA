using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

[AllowAnonymous]
public class CuentaController : Controller
{
    private readonly PedidoHubDbContext _context;
    private readonly IPasswordHasher<Usuario> _hasher;

    public CuentaController(PedidoHubDbContext context, IPasswordHasher<Usuario> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Ingresar(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Servicio", "Home");
        }
        return View(new IngresarViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingresar(IngresarViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var nombre = modelo.NombreUsuario.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.NombreUsuario == nombre);
        var valido = usuario is not null &&
            _hasher.VerifyHashedPassword(usuario, usuario.ClaveHash, modelo.Clave) != PasswordVerificationResult.Failed;
        if (!valido)
        {
            // Mensaje genérico: no revela si el usuario existe.
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            modelo.Clave = string.Empty;
            return View(modelo);
        }

        var identidad = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, usuario!.NombreUsuario),
            new Claim("NombreCompleto", usuario.NombreCompleto),
            new Claim(ClaimTypes.Role, usuario.Rol)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));

        return !string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl)
            ? LocalRedirect(modelo.ReturnUrl)
            : RedirectToAction("Servicio", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denegado() => View();
}