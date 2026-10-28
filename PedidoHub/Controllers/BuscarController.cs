using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;

namespace PedidoHub.Controllers;

public record ResultadoBusqueda(string Tipo, string Titulo, string Detalle, string Url);

// Alimenta la paleta de comandos (Ctrl+K): devuelve coincidencias de toda la aplicación.
public class BuscarController : Controller
{
    private const int MinimoCaracteres = 2;
    private const int PorTipo = 4;

    private readonly PedidoHubDbContext _context;

    public BuscarController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        q = q?.Trim();
        var resultados = new List<ResultadoBusqueda>();
        if (string.IsNullOrEmpty(q) || q.Length < MinimoCaracteres)
        {
            return Json(resultados);
        }

        var clientes = await _context.Clientes.AsNoTracking()
            .Where(c => c.Nombre.Contains(q) || c.Telefonos.Any(t => t.Numero.Contains(q)))
            .OrderBy(c => c.Nombre)
            .Take(PorTipo)
            .Select(c => new { c.IdCliente, c.Nombre, Telefonos = c.Telefonos.Count })
            .ToListAsync();
        resultados.AddRange(clientes.Select(c => new ResultadoBusqueda(
            "Cliente", c.Nombre, $"{c.Telefonos} {(c.Telefonos == 1 ? "teléfono" : "teléfonos")}",
            Url.Action("Details", "Clientes", new { id = c.IdCliente })!)));

        var platos = await _context.Platos.AsNoTracking()
            .Where(p => p.Nombre.Contains(q))
            .OrderBy(p => p.Nombre)
            .Take(PorTipo)
            .ToListAsync();
        resultados.AddRange(platos.Select(p => new ResultadoBusqueda(
            "Plato", p.Nombre, p.Precio.ToString("N2"),
            Url.Action("Edit", "Platos", new { id = p.CodPlato })!)));

        var empleados = await _context.Empleados.AsNoTracking()
            .Where(e => e.Nombre.Contains(q) || e.Cargo.Contains(q))
            .OrderBy(e => e.Nombre)
            .Take(PorTipo)
            .ToListAsync();
        resultados.AddRange(empleados.Select(e => new ResultadoBusqueda(
            "Empleado", e.Nombre, e.Cargo,
            Url.Action("Edit", "Empleados", new { id = e.IdEmpleado })!)));

        // Un número encuentra la mesa, el pedido y la factura con ese número.
        if (int.TryParse(q.TrimStart('#', 'N', 'n', '°', ' '), out var numero))
        {
            if (await _context.Mesas.AnyAsync(m => m.NumMesa == numero))
            {
                resultados.Add(new ResultadoBusqueda("Mesa", $"Mesa {numero}", "Abrir pedido en esta mesa",
                    Url.Action("Create", "Pedidos", new { numMesa = numero })!));
            }
            if (await _context.Pedidos.AnyAsync(p => p.IdPedido == numero))
            {
                resultados.Add(new ResultadoBusqueda("Pedido", $"Pedido N° {numero}", "Ver detalle y platos",
                    Url.Action("Details", "Pedidos", new { id = numero })!));
            }
            if (await _context.Facturas.AnyAsync(f => f.NumFact == numero))
            {
                resultados.Add(new ResultadoBusqueda("Factura", $"Factura N° {numero}", "Ver factura",
                    Url.Action("Details", "Facturas", new { id = numero })!));
            }
        }

        return Json(resultados);
    }
}
