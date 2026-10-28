using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

public class PedidosController : Controller
{
    private readonly PedidoHubDbContext _context;

    public PedidosController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, EstadoPedido? estado)
    {
        var consulta = _context.Pedidos.AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Empleado)
            .Include(p => p.Factura)
            .Include(p => p.Detalles).ThenInclude(d => d.Plato)
            .AsQueryable();

        if (desde.HasValue)
        {
            consulta = consulta.Where(p => p.FechaHora >= desde.Value.Date);
        }
        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(p => p.FechaHora < limite);
        }
        // Conteo por estado dentro del período, para las pestañas de la lista.
        var conteos = await consulta.GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToListAsync();
        ViewData["Conteos"] = conteos.ToDictionary(c => c.Estado, c => c.Total);

        if (estado.HasValue)
        {
            consulta = consulta.Where(p => p.Estado == estado.Value);
        }

        ViewData["Desde"] = desde;
        ViewData["Hasta"] = hasta;
        ViewData["Estado"] = estado;
        return View(await consulta.OrderByDescending(p => p.FechaHora).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var modelo = await ConstruirDetalleAsync(id.Value);
        if (modelo is null)
        {
            return NotFound();
        }

        return View(modelo);
    }

    public async Task<IActionResult> Create(int? numMesa)
    {
        var modelo = new PedidoFormViewModel { NumMesa = numMesa ?? 0 };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoFormViewModel modelo)
    {
        ValidarEstado(modelo, tieneFactura: false);
        await ValidarMesaLibreAsync(modelo);
        if (!ModelState.IsValid)
        {
            await CargarListasAsync(modelo);
            return View(modelo);
        }

        var pedido = new Pedido
        {
            FechaHora = modelo.FechaHora,
            Estado = modelo.Estado,
            NumMesa = modelo.NumMesa,
            IdEmpleado = modelo.IdEmpleado
        };
        _context.Add(pedido);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Pedido creado. Agregue los platos que se solicitaron.";
        return RedirectToAction(nameof(Details), new { id = pedido.IdPedido });
    }

    // Abre un pedido en una mesa libre con un solo paso (desde el cajón del plano de mesas).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AbrirRapido(int numMesa, int idEmpleado)
    {
        if (!await _context.Mesas.AnyAsync(m => m.NumMesa == numMesa) ||
            !await _context.Empleados.AnyAsync(e => e.IdEmpleado == idEmpleado))
        {
            TempData["Error"] = "Seleccione una mesa y un empleado válidos para abrir el pedido.";
            return RedirectToAction("Servicio", "Home");
        }

        var ocupada = await _context.Pedidos.AnyAsync(p => p.NumMesa == numMesa && p.Factura == null &&
            (p.Estado == EstadoPedido.Abierto || p.Estado == EstadoPedido.Servido));
        if (ocupada)
        {
            TempData["Error"] = $"La mesa {numMesa} ya tiene un pedido sin facturar.";
            return RedirectToAction("Servicio", "Home");
        }

        var pedido = new Pedido
        {
            FechaHora = DateTime.Now,
            Estado = EstadoPedido.Abierto,
            NumMesa = numMesa,
            IdEmpleado = idEmpleado
        };
        _context.Add(pedido);
        await _context.SaveChangesAsync();
        TempData["Exito"] = $"Pedido abierto en la mesa {numMesa}. Agregue los platos.";
        return RedirectToAction(nameof(Details), new { id = pedido.IdPedido });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var pedido = await _context.Pedidos.AsNoTracking()
            .Include(p => p.Factura)
            .FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return NotFound();
        }
        if (pedido.Factura is not null)
        {
            TempData["Error"] = "Un pedido facturado no puede modificarse. Elimine primero su factura.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var modelo = new PedidoFormViewModel
        {
            IdPedido = pedido.IdPedido,
            FechaHora = pedido.FechaHora,
            Estado = pedido.Estado,
            NumMesa = pedido.NumMesa,
            IdEmpleado = pedido.IdEmpleado
        };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PedidoFormViewModel modelo)
    {
        if (id != modelo.IdPedido)
        {
            return NotFound();
        }

        var pedido = await _context.Pedidos.Include(p => p.Factura).FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return NotFound();
        }
        if (pedido.Factura is not null)
        {
            TempData["Error"] = "Un pedido facturado no puede modificarse. Elimine primero su factura.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ValidarEstado(modelo, tieneFactura: false);
        await ValidarMesaLibreAsync(modelo);
        if (!ModelState.IsValid)
        {
            await CargarListasAsync(modelo);
            return View(modelo);
        }

        pedido.FechaHora = modelo.FechaHora;
        pedido.Estado = modelo.Estado;
        pedido.NumMesa = modelo.NumMesa;
        pedido.IdEmpleado = modelo.IdEmpleado;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Pedido actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var pedido = await _context.Pedidos.AsNoTracking()
            .Include(p => p.Mesa)
            .FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return NotFound();
        }

        return View("~/Views/Shared/ConfirmarEliminar.cshtml", new ConfirmarEliminarViewModel
        {
            Id = pedido.IdPedido,
            Titulo = "Eliminar pedido",
            Descripcion = $"Se eliminará el pedido N° {pedido.IdPedido} de la mesa {pedido.NumMesa} junto con sus platos.",
            UrlVolver = Url.Action(nameof(Index))!
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pedido = await _context.Pedidos.Include(p => p.Factura).FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return NotFound();
        }
        if (pedido.Factura is not null)
        {
            TempData["Error"] = $"No se puede eliminar el pedido {id} porque tiene la factura N° {pedido.Factura.NumFact}.";
            return RedirectToAction(nameof(Index));
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Pedido eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarPlato([Bind("IdPedido,CodPlato,Cantidad", Prefix = "NuevoPlato")] AgregarPlatoViewModel nuevo)
    {
        var pedido = await _context.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.IdPedido == nuevo.IdPedido);
        if (pedido is null)
        {
            return NotFound();
        }
        if (!PuedeModificarse(pedido))
        {
            TempData["Error"] = "Solo se pueden modificar los platos de pedidos abiertos o servidos.";
            return RedirectToAction(nameof(Details), new { id = pedido.IdPedido });
        }
        if (!ModelState.IsValid)
        {
            var detalle = await ConstruirDetalleAsync(pedido.IdPedido);
            detalle!.NuevoPlato = nuevo;
            await CargarPlatosAsync(detalle.NuevoPlato);
            return View(nameof(Details), detalle);
        }

        // Si el plato ya está en el pedido se suma la cantidad (clave compuesta IdPedido + CodPlato).
        var existente = pedido.Detalles.FirstOrDefault(d => d.CodPlato == nuevo.CodPlato);
        if (existente is null)
        {
            _context.PedidoPlatos.Add(new PedidoPlato { IdPedido = pedido.IdPedido, CodPlato = nuevo.CodPlato, Cantidad = nuevo.Cantidad });
        }
        else
        {
            existente.Cantidad += nuevo.Cantidad;
        }

        await _context.SaveChangesAsync();
        TempData["Exito"] = "Plato agregado al pedido.";
        return RedirectToAction(nameof(Details), new { id = pedido.IdPedido });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarPlato(int idPedido, int codPlato)
    {
        var detalle = await _context.PedidoPlatos
            .Include(d => d.Pedido)
            .FirstOrDefaultAsync(d => d.IdPedido == idPedido && d.CodPlato == codPlato);
        if (detalle is null)
        {
            return NotFound();
        }
        if (!PuedeModificarse(detalle.Pedido))
        {
            TempData["Error"] = "Solo se pueden modificar los platos de pedidos abiertos o servidos.";
            return RedirectToAction(nameof(Details), new { id = idPedido });
        }

        _context.PedidoPlatos.Remove(detalle);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Plato quitado del pedido.";
        return RedirectToAction(nameof(Details), new { id = idPedido });
    }

    private static bool PuedeModificarse(Pedido pedido) =>
        pedido.Estado == EstadoPedido.Abierto || pedido.Estado == EstadoPedido.Servido;

    // Una mesa no puede tener dos pedidos en curso a la vez (misma regla que AbrirRapido).
    private async Task ValidarMesaLibreAsync(PedidoFormViewModel modelo)
    {
        if (modelo.Estado is not (EstadoPedido.Abierto or EstadoPedido.Servido))
        {
            return;
        }

        var ocupada = await _context.Pedidos.AnyAsync(p =>
            p.IdPedido != modelo.IdPedido && p.NumMesa == modelo.NumMesa && p.Factura == null &&
            (p.Estado == EstadoPedido.Abierto || p.Estado == EstadoPedido.Servido));
        if (ocupada)
        {
            ModelState.AddModelError(nameof(modelo.NumMesa), $"La mesa {modelo.NumMesa} ya tiene un pedido sin facturar.");
        }
    }

    private void ValidarEstado(PedidoFormViewModel modelo, bool tieneFactura)
    {
        if (modelo.Estado == EstadoPedido.Facturado && !tieneFactura)
        {
            ModelState.AddModelError(nameof(modelo.Estado),
                "El estado \"Facturado\" se asigna automáticamente al generar la factura del pedido.");
        }
    }

    private async Task<PedidoDetalleViewModel?> ConstruirDetalleAsync(int id)
    {
        var pedido = await _context.Pedidos.AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Empleado)
            .Include(p => p.Factura)
            .Include(p => p.Detalles).ThenInclude(d => d.Plato)
            .FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido is null)
        {
            return null;
        }

        var modelo = new PedidoDetalleViewModel
        {
            Pedido = pedido,
            NuevoPlato = new AgregarPlatoViewModel { IdPedido = id }
        };
        await CargarPlatosAsync(modelo.NuevoPlato);
        return modelo;
    }

    private async Task CargarPlatosAsync(AgregarPlatoViewModel modelo)
    {
        modelo.Platos = await _context.Platos.AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem($"{p.Nombre} - {p.Precio:N2}", p.CodPlato.ToString()))
            .ToListAsync();
    }

    private async Task CargarListasAsync(PedidoFormViewModel modelo)
    {
        modelo.Mesas = await _context.Mesas.AsNoTracking()
            .OrderBy(m => m.NumMesa)
            .Select(m => new SelectListItem($"Mesa {m.NumMesa} (capacidad {m.Capacidad})", m.NumMesa.ToString()))
            .ToListAsync();
        modelo.Empleados = await _context.Empleados.AsNoTracking()
            .OrderBy(e => e.Nombre)
            .Select(e => new SelectListItem($"{e.Nombre} ({e.Cargo})", e.IdEmpleado.ToString()))
            .ToListAsync();
    }
}
