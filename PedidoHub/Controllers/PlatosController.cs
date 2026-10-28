using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class PlatosController : Controller
{
    private readonly PedidoHubDbContext _context;

    public PlatosController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Platos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync());
    }

    public IActionResult Create()
    {
        return View(new Plato());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Precio")] Plato plato)
    {
        plato.Nombre = plato.Nombre?.Trim() ?? string.Empty;
        await ValidarNombreUnicoAsync(plato);
        if (!ModelState.IsValid)
        {
            return View(plato);
        }

        _context.Add(plato);
        await _context.SaveChangesAsync();
        TempData["Exito"] = $"Plato \"{plato.Nombre}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var plato = await _context.Platos.FindAsync(id);
        if (plato is null)
        {
            return NotFound();
        }

        return View(plato);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CodPlato,Nombre,Precio")] Plato plato)
    {
        if (id != plato.CodPlato)
        {
            return NotFound();
        }

        plato.Nombre = plato.Nombre?.Trim() ?? string.Empty;
        await ValidarNombreUnicoAsync(plato);
        if (!ModelState.IsValid)
        {
            return View(plato);
        }

        var existente = await _context.Platos.FindAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Nombre = plato.Nombre;
        existente.Precio = plato.Precio;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Plato actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var plato = await _context.Platos.AsNoTracking().FirstOrDefaultAsync(p => p.CodPlato == id);
        if (plato is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = plato.CodPlato,
            Titulo = "Eliminar plato",
            Descripcion = $"Se eliminará el plato \"{plato.Nombre}\" del menú.",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var plato = await _context.Platos.FindAsync(id);
        if (plato is null)
        {
            return NotFound();
        }

        if (await _context.PedidoPlatos.AnyAsync(d => d.CodPlato == id))
        {
            TempData["Error"] = $"No se puede eliminar \"{plato.Nombre}\" porque forma parte de pedidos registrados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Platos.Remove(plato);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Plato eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarNombreUnicoAsync(Plato plato)
    {
        if (await _context.Platos.AnyAsync(p => p.Nombre == plato.Nombre && p.CodPlato != plato.CodPlato))
        {
            ModelState.AddModelError(nameof(plato.Nombre), "Ya existe un plato con ese nombre.");
        }
    }
}
