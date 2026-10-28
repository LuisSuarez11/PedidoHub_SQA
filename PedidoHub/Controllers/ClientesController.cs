using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class ClientesController : Controller
{
    private readonly PedidoHubDbContext _context;

    public ClientesController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var clientes = await _context.Clientes.AsNoTracking()
            .Include(c => c.Telefonos)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        return View(clientes);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.AsNoTracking()
            .Include(c => c.Telefonos)
            .Include(c => c.Reservas).ThenInclude(r => r.Mesa)
            .FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente is null)
        {
            return NotFound();
        }

        return View(cliente);
    }

    public IActionResult Create()
    {
        return View(new ClienteFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormViewModel modelo)
    {
        modelo.Nombre = modelo.Nombre?.Trim() ?? string.Empty;
        modelo.Telefono = modelo.Telefono?.Trim() ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        // El cliente nace con su primer teléfono (DER: un cliente posee de 1 a n teléfonos).
        var cliente = new Cliente
        {
            Nombre = modelo.Nombre,
            Telefonos = { new TelefonoCliente { Numero = modelo.Telefono } }
        };
        _context.Add(cliente);
        await _context.SaveChangesAsync();
        TempData["Exito"] = $"Cliente \"{cliente.Nombre}\" registrado. Puede agregar más teléfonos desde su ficha.";
        return RedirectToAction(nameof(Details), new { id = cliente.IdCliente });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdCliente,Nombre")] Cliente cliente)
    {
        if (id != cliente.IdCliente)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(cliente);
        }

        var existente = await _context.Clientes.FindAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Nombre = cliente.Nombre;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.AsNoTracking()
            .Include(c => c.Telefonos)
            .FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = cliente.IdCliente,
            Titulo = "Eliminar cliente",
            Descripcion = $"Se eliminará al cliente \"{cliente.Nombre}\" y sus {cliente.Telefonos.Count} teléfono(s).",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        if (await _context.Reservas.AnyAsync(r => r.IdCliente == id))
        {
            TempData["Error"] = $"No se puede eliminar a \"{cliente.Nombre}\" porque tiene reservas registradas.";
            return RedirectToAction(nameof(Index));
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Cliente eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
