using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = PedidoHub.Models.Roles.Administrador + "," + PedidoHub.Models.Roles.Cajero)]
public class FacturasController : Controller
{
    private readonly PedidoHubDbContext _context;

    public FacturasController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
    {
        var consulta = _context.Facturas.AsNoTracking()
            .Include(f => f.Pedido).ThenInclude(p => p.Mesa)
            .Include(f => f.Pedido).ThenInclude(p => p.Empleado)
            .Include(f => f.Pedido).ThenInclude(p => p.Detalles).ThenInclude(d => d.Plato)
            .AsQueryable();

        if (desde.HasValue)
        {
            consulta = consulta.Where(f => f.FechaEmision >= desde.Value.Date);
        }
        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(f => f.FechaEmision < limite);
        }

        ViewData["Desde"] = desde;
        ViewData["Hasta"] = hasta;
        return View(await consulta.OrderByDescending(f => f.FechaEmision).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var factura = await _context.Facturas.AsNoTracking()
            .Include(f => f.Pedido).ThenInclude(p => p.Mesa)
            .Include(f => f.Pedido).ThenInclude(p => p.Empleado)
            .Include(f => f.Pedido).ThenInclude(p => p.Detalles).ThenInclude(d => d.Plato)
            .FirstOrDefaultAsync(f => f.NumFact == id);
        if (factura is null)
        {
            return NotFound();
        }

        return View(factura);
    }

    public async Task<IActionResult> Create(int? idPedido)
    {
        if (idPedido is null)
        {
            return NotFound();
        }

        var pedido = await CargarPedidoAsync(idPedido.Value);
        if (pedido is null)
        {
            return NotFound();
        }

        var error = ValidarPedidoFacturable(pedido);
        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Details", "Pedidos", new { id = idPedido });
        }

        return View(ConstruirFormulario(pedido, new FacturaFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FacturaFormViewModel modelo)
    {
        var pedido = await CargarPedidoAsync(modelo.IdPedido);
        if (pedido is null)
        {
            return NotFound();
        }

        var error = ValidarPedidoFacturable(pedido);
        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Details", "Pedidos", new { id = modelo.IdPedido });
        }
        if (modelo.FechaEmision < pedido.FechaHora)
        {
            ModelState.AddModelError(nameof(modelo.FechaEmision),
                "La fecha de emisión no puede ser anterior a la del pedido.");
        }
        if (!ModelState.IsValid)
        {
            return View(ConstruirFormulario(pedido, modelo));
        }

        var factura = new Factura { IdPedido = pedido.IdPedido, NIT = modelo.NIT, FechaEmision = modelo.FechaEmision };
        pedido.Estado = EstadoPedido.Facturado;
        _context.Add(factura);
        await _context.SaveChangesAsync();
        TempData["Exito"] = $"Factura N° {factura.NumFact} generada correctamente.";
        return RedirectToAction(nameof(Details), new { id = factura.NumFact });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var factura = await _context.Facturas.AsNoTracking().FirstOrDefaultAsync(f => f.NumFact == id);
        if (factura is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = factura.NumFact,
            Titulo = "Anular factura",
            Descripcion = $"Se eliminará la factura N° {factura.NumFact} y el pedido {factura.IdPedido} volverá al estado \"Servido\".",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var factura = await _context.Facturas.Include(f => f.Pedido).FirstOrDefaultAsync(f => f.NumFact == id);
        if (factura is null)
        {
            return NotFound();
        }

        factura.Pedido.Estado = EstadoPedido.Servido;
        _context.Facturas.Remove(factura);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Factura anulada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private Task<Pedido?> CargarPedidoAsync(int idPedido) =>
        _context.Pedidos
            .Include(p => p.Mesa)
            .Include(p => p.Factura)
            .Include(p => p.Detalles).ThenInclude(d => d.Plato)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

    private static string? ValidarPedidoFacturable(Pedido pedido)
    {
        if (pedido.Factura is not null)
        {
            return $"El pedido {pedido.IdPedido} ya tiene la factura N° {pedido.Factura.NumFact}.";
        }
        if (pedido.Estado == EstadoPedido.Cancelado)
        {
            return "No se puede facturar un pedido cancelado.";
        }
        if (pedido.Detalles.Count == 0)
        {
            return "El pedido no tiene platos. Agregue al menos uno antes de facturar.";
        }

        return null;
    }

    private static FacturaFormViewModel ConstruirFormulario(Pedido pedido, FacturaFormViewModel modelo)
    {
        modelo.IdPedido = pedido.IdPedido;
        modelo.DescripcionPedido = $"Pedido N° {pedido.IdPedido} - Mesa {pedido.NumMesa} - {pedido.FechaHora:dd/MM/yyyy HH:mm}";
        modelo.Total = pedido.Total;
        return modelo;
    }
}
