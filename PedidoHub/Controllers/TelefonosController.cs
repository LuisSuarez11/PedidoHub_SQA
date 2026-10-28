using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class TelefonosController : Controller
{
    private const string MensajeUltimoTelefono =
        "No se puede eliminar el único teléfono del cliente: debe conservar al menos uno. Agregue otro antes de eliminar este.";

    private readonly PedidoHubDbContext _context;

    public TelefonosController(PedidoHubDbContext context)
    {
        _context = context;
    }

    // DER: un cliente posee de 1 a n teléfonos, así que el último no puede eliminarse.
    private async Task<bool> EsUltimoTelefonoAsync(int idCliente) =>
        await _context.Telefonos.CountAsync(t => t.IdCliente == idCliente) <= 1;

    public async Task<IActionResult> Create(int? idCliente)
    {
        if (idCliente is null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == idCliente);
        if (cliente is null)
        {
            return NotFound();
        }

        ViewData["NombreCliente"] = cliente.Nombre;
        return View(new TelefonoCliente { IdCliente = cliente.IdCliente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdCliente,Numero")] TelefonoCliente telefono)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == telefono.IdCliente);
        if (cliente is null)
        {
            return NotFound();
        }

        telefono.Numero = telefono.Numero?.Trim() ?? string.Empty;
        if (ModelState.IsValid && await _context.Telefonos.AnyAsync(t => t.IdCliente == telefono.IdCliente && t.Numero == telefono.Numero))
        {
            ModelState.AddModelError(nameof(telefono.Numero), "Este cliente ya tiene registrado ese número.");
        }
        if (!ModelState.IsValid)
        {
            ViewData["NombreCliente"] = cliente.Nombre;
            return View(telefono);
        }

        _context.Add(telefono);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Teléfono agregado correctamente.";
        return RedirectToAction("Details", "Clientes", new { id = telefono.IdCliente });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var telefono = await _context.Telefonos.Include(t => t.Cliente).FirstOrDefaultAsync(t => t.IdTelefono == id);
        if (telefono is null)
        {
            return NotFound();
        }

        ViewData["NombreCliente"] = telefono.Cliente!.Nombre;
        return View(telefono);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdTelefono,Numero")] TelefonoCliente modelo)
    {
        if (id != modelo.IdTelefono)
        {
            return NotFound();
        }

        var telefono = await _context.Telefonos.Include(t => t.Cliente).FirstOrDefaultAsync(t => t.IdTelefono == id);
        if (telefono is null)
        {
            return NotFound();
        }

        modelo.Numero = modelo.Numero?.Trim() ?? string.Empty;
        if (ModelState.IsValid && await _context.Telefonos.AnyAsync(t =>
                t.IdCliente == telefono.IdCliente && t.Numero == modelo.Numero && t.IdTelefono != id))
        {
            ModelState.AddModelError(nameof(modelo.Numero), "Este cliente ya tiene registrado ese número.");
        }
        if (!ModelState.IsValid)
        {
            ViewData["NombreCliente"] = telefono.Cliente!.Nombre;
            modelo.IdCliente = telefono.IdCliente;
            return View(modelo);
        }

        telefono.Numero = modelo.Numero;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Teléfono actualizado correctamente.";
        return RedirectToAction("Details", "Clientes", new { id = telefono.IdCliente });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var telefono = await _context.Telefonos.AsNoTracking()
            .Include(t => t.Cliente)
            .FirstOrDefaultAsync(t => t.IdTelefono == id);
        if (telefono is null)
        {
            return NotFound();
        }

        if (await EsUltimoTelefonoAsync(telefono.IdCliente))
        {
            TempData["Error"] = MensajeUltimoTelefono;
            return RedirectToAction("Details", "Clientes", new { id = telefono.IdCliente });
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = telefono.IdTelefono,
            Titulo = "Eliminar teléfono",
            Descripcion = $"Se eliminará el teléfono {telefono.Numero} de {telefono.Cliente!.Nombre}.",
            UrlVolver = Url.Action("Details", "Clientes", new { id = telefono.IdCliente })!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var telefono = await _context.Telefonos.FindAsync(id);
        if (telefono is null)
        {
            return NotFound();
        }

        var idCliente = telefono.IdCliente;
        if (await EsUltimoTelefonoAsync(idCliente))
        {
            TempData["Error"] = MensajeUltimoTelefono;
            return RedirectToAction("Details", "Clientes", new { id = idCliente });
        }

        _context.Telefonos.Remove(telefono);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Teléfono eliminado correctamente.";
        return RedirectToAction("Details", "Clientes", new { id = idCliente });
    }
}
