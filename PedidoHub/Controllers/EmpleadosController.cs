using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class EmpleadosController : Controller
{
    private readonly PedidoHubDbContext _context;

    public EmpleadosController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Empleados.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync());
    }

    public IActionResult Create()
    {
        return View(new Empleado());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Cargo")] Empleado empleado)
    {
        if (!ModelState.IsValid)
        {
            return View(empleado);
        }

        _context.Add(empleado);
        await _context.SaveChangesAsync();
        TempData["Exito"] = $"Empleado \"{empleado.Nombre}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado is null)
        {
            return NotFound();
        }

        return View(empleado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdEmpleado,Nombre,Cargo")] Empleado empleado)
    {
        if (id != empleado.IdEmpleado)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(empleado);
        }

        var existente = await _context.Empleados.FindAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Nombre = empleado.Nombre;
        existente.Cargo = empleado.Cargo;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Empleado actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var empleado = await _context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.IdEmpleado == id);
        if (empleado is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = empleado.IdEmpleado,
            Titulo = "Eliminar empleado",
            Descripcion = $"Se eliminará a \"{empleado.Nombre}\" ({empleado.Cargo}).",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado is null)
        {
            return NotFound();
        }

        if (await _context.Pedidos.AnyAsync(p => p.IdEmpleado == id))
        {
            TempData["Error"] = $"No se puede eliminar a \"{empleado.Nombre}\" porque tiene pedidos atendidos.";
            return RedirectToAction(nameof(Index));
        }

        _context.Empleados.Remove(empleado);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Empleado eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
