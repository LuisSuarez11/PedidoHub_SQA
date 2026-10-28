using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class MesasController : Controller
{
    private readonly PedidoHubDbContext _context;

    public MesasController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Mesas.AsNoTracking().OrderBy(m => m.NumMesa).ToListAsync());
    }

    // Fragmento HTML para el cajón lateral del plano de mesas.
    public async Task<IActionResult> Panel(int id)
    {
        var mesa = await _context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.NumMesa == id);
        if (mesa is null)
        {
            return NotFound();
        }

        var pedido = await _context.Pedidos.AsNoTracking()
            .Include(p => p.Empleado)
            .Include(p => p.Detalles).ThenInclude(d => d.Plato)
            .Where(p => p.NumMesa == id && p.Factura == null &&
                        (p.Estado == EstadoPedido.Abierto || p.Estado == EstadoPedido.Servido))
            .OrderBy(p => p.FechaHora)
            .FirstOrDefaultAsync();

        var reservas = await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Where(r => r.NumMesa == id && r.FechaHora >= DateTime.Today &&
                        (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada))
            .OrderBy(r => r.FechaHora)
            .Take(3)
            .ToListAsync();

        var empleados = await _context.Empleados.AsNoTracking()
            .OrderBy(e => e.Nombre)
            .Select(e => new SelectListItem($"{e.Nombre} ({e.Cargo})", e.IdEmpleado.ToString()))
            .ToListAsync();

        return PartialView("_Panel", new MesaPanelViewModel
        {
            Mesa = mesa,
            Pedido = pedido,
            ProximasReservas = reservas,
            Empleados = empleados
        });
    }

    public IActionResult Create()
    {
        return View(new Mesa());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Capacidad")] Mesa mesa)
    {
        if (!ModelState.IsValid)
        {
            return View(mesa);
        }

        _context.Add(mesa);
        await _context.SaveChangesAsync();
        TempData["Exito"] = $"Mesa {mesa.NumMesa} registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa is null)
        {
            return NotFound();
        }

        return View(mesa);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NumMesa,Capacidad")] Mesa mesa)
    {
        if (id != mesa.NumMesa)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(mesa);
        }

        var existente = await _context.Mesas.FindAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Capacidad = mesa.Capacidad;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Mesa actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var mesa = await _context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.NumMesa == id);
        if (mesa is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = mesa.NumMesa,
            Titulo = "Eliminar mesa",
            Descripcion = $"Se eliminará la mesa {mesa.NumMesa} (capacidad: {mesa.Capacidad} personas).",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa is null)
        {
            return NotFound();
        }

        if (await _context.Reservas.AnyAsync(r => r.NumMesa == id) || await _context.Pedidos.AnyAsync(p => p.NumMesa == id))
        {
            TempData["Error"] = $"No se puede eliminar la mesa {id} porque tiene reservas o pedidos asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Mesas.Remove(mesa);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Mesa eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
