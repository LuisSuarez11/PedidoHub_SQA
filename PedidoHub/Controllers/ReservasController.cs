using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class ReservasController : Controller
{
    private const int HorasPorReserva = 2;

    private readonly PedidoHubDbContext _context;

    public ReservasController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, EstadoReserva? estado)
    {
        var consulta = _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.Mesa)
            .AsQueryable();

        if (desde.HasValue)
        {
            consulta = consulta.Where(r => r.FechaHora >= desde.Value.Date);
        }
        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(r => r.FechaHora < limite);
        }
        // Conteo por estado dentro del período, para las pestañas de la lista.
        var conteos = await consulta.GroupBy(r => r.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToListAsync();
        ViewData["Conteos"] = conteos.ToDictionary(c => c.Estado, c => c.Total);

        if (estado.HasValue)
        {
            consulta = consulta.Where(r => r.Estado == estado.Value);
        }

        ViewData["Desde"] = desde;
        ViewData["Hasta"] = hasta;
        ViewData["Estado"] = estado;
        return View(await consulta.OrderByDescending(r => r.FechaHora).ToListAsync());
    }

    public async Task<IActionResult> Create(int? idCliente)
    {
        var modelo = new ReservaFormViewModel { IdCliente = idCliente ?? 0 };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservaFormViewModel modelo)
    {
        if (modelo.FechaHora < DateTime.Now)
        {
            ModelState.AddModelError(nameof(modelo.FechaHora),
                "La fecha de una reserva nueva no puede estar en el pasado.");
        }
        await ValidarReservaAsync(modelo);
        if (!ModelState.IsValid)
        {
            await CargarListasAsync(modelo);
            return View(modelo);
        }

        _context.Add(new Reserva
        {
            FechaHora = modelo.FechaHora,
            CantPersonas = modelo.CantPersonas,
            Estado = modelo.Estado,
            IdCliente = modelo.IdCliente,
            NumMesa = modelo.NumMesa
        });
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Reserva registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas.AsNoTracking().FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null)
        {
            return NotFound();
        }

        var modelo = new ReservaFormViewModel
        {
            IdReserva = reserva.IdReserva,
            FechaHora = reserva.FechaHora,
            CantPersonas = reserva.CantPersonas,
            Estado = reserva.Estado,
            IdCliente = reserva.IdCliente,
            NumMesa = reserva.NumMesa
        };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReservaFormViewModel modelo)
    {
        if (id != modelo.IdReserva)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null)
        {
            return NotFound();
        }

        await ValidarReservaAsync(modelo);
        if (!ModelState.IsValid)
        {
            await CargarListasAsync(modelo);
            return View(modelo);
        }

        reserva.FechaHora = modelo.FechaHora;
        reserva.CantPersonas = modelo.CantPersonas;
        reserva.Estado = modelo.Estado;
        reserva.IdCliente = modelo.IdCliente;
        reserva.NumMesa = modelo.NumMesa;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Reserva actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = reserva.IdReserva,
            Titulo = "Eliminar reserva",
            Descripcion = $"Se eliminará la reserva de {reserva.Cliente.Nombre} del {reserva.FechaHora:dd/MM/yyyy HH:mm} (mesa {reserva.NumMesa}).",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null)
        {
            return NotFound();
        }

        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Reserva eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarReservaAsync(ReservaFormViewModel modelo)
    {
        var mesa = await _context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.NumMesa == modelo.NumMesa);
        if (mesa is not null && modelo.CantPersonas > mesa.Capacidad)
        {
            ModelState.AddModelError(nameof(modelo.CantPersonas),
                $"La mesa {mesa.NumMesa} tiene capacidad para {mesa.Capacidad} personas.");
        }

        if (modelo.Estado == EstadoReserva.Cancelada || modelo.NumMesa == 0)
        {
            return;
        }

        // Una mesa no puede tener dos reservas activas que se superpongan en horario.
        var inicio = modelo.FechaHora.AddHours(-HorasPorReserva);
        var fin = modelo.FechaHora.AddHours(HorasPorReserva);
        var hayConflicto = await _context.Reservas.AnyAsync(r =>
            r.IdReserva != modelo.IdReserva &&
            r.NumMesa == modelo.NumMesa &&
            r.Estado != EstadoReserva.Cancelada &&
            r.FechaHora > inicio && r.FechaHora < fin);
        if (hayConflicto)
        {
            ModelState.AddModelError(nameof(modelo.NumMesa),
                $"La mesa {modelo.NumMesa} ya tiene una reserva en un horario cercano (menos de {HorasPorReserva} horas).");
        }
    }

    private async Task CargarListasAsync(ReservaFormViewModel modelo)
    {
        modelo.Clientes = await _context.Clientes.AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.IdCliente.ToString()))
            .ToListAsync();
        modelo.Mesas = await _context.Mesas.AsNoTracking()
            .OrderBy(m => m.NumMesa)
            .Select(m => new SelectListItem($"Mesa {m.NumMesa} (capacidad {m.Capacidad})", m.NumMesa.ToString()))
            .ToListAsync();
    }
}
