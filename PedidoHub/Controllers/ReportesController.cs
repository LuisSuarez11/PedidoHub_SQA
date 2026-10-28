using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Services;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class ReportesController : Controller
{
    private readonly PedidoHubDbContext _context;
    private readonly ReporteService _reportes;

    public ReportesController(PedidoHubDbContext context, ReporteService reportes)
    {
        _context = context;
        _reportes = reportes;
    }

    public async Task<IActionResult> Ventas(ReporteVentasViewModel modelo)
    {
        if (modelo.Desde > modelo.Hasta)
        {
            ModelState.AddModelError(nameof(modelo.Hasta), "La fecha final no puede ser anterior a la inicial.");
        }
        else if (ModelState.IsValid)
        {
            modelo.Filas = await _reportes.ObtenerVentasAsync(modelo.Desde, modelo.Hasta, modelo.IdEmpleado);
        }

        modelo.Empleados = await _context.Empleados.AsNoTracking()
            .OrderBy(e => e.Nombre)
            .Select(e => new SelectListItem(e.Nombre, e.IdEmpleado.ToString()))
            .ToListAsync();
        return View(modelo);
    }

    public async Task<IActionResult> PlatosMasVendidos(ReportePlatosViewModel modelo)
    {
        if (modelo.Desde > modelo.Hasta)
        {
            ModelState.AddModelError(nameof(modelo.Hasta), "La fecha final no puede ser anterior a la inicial.");
        }
        else if (ModelState.IsValid)
        {
            modelo.Filas = await _reportes.ObtenerPlatosMasVendidosAsync(modelo.Desde, modelo.Hasta, modelo.Top);
        }

        return View(modelo);
    }
}
