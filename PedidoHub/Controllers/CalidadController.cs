using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.Services;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

/// <summary>Panel de calidad: métricas del Test Plan ágil junto al estado vivo del sistema.</summary>
[Authorize(Roles = Roles.Administrador)]
public class CalidadController : Controller
{
    private readonly PedidoHubDbContext _context;
    private readonly CalidadService _calidad;

    public CalidadController(PedidoHubDbContext context, CalidadService calidad)
    {
        _context = context;
        _calidad = calidad;
    }

    public async Task<IActionResult> Index()
    {
        var modelo = new CalidadViewModel { Datos = _calidad.Leer() };
        modelo.BaseDeDatosDisponible = await _context.Database.CanConnectAsync();
        if (modelo.BaseDeDatosDisponible)
        {
            modelo.Registros = new Dictionary<string, int>
            {
                ["Clientes"] = await _context.Clientes.CountAsync(),
                ["Reservas"] = await _context.Reservas.CountAsync(),
                ["Pedidos"] = await _context.Pedidos.CountAsync(),
                ["Facturas"] = await _context.Facturas.CountAsync(),
                ["Cambios auditados"] = await _context.Bitacoras.CountAsync()
            };
        }
        return View(modelo);
    }
}
