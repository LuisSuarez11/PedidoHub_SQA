using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Services;
using PedidoHub.ViewModels;

namespace PedidoHub.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = PedidoHub.Models.Roles.Administrador + "," + PedidoHub.Models.Roles.Cajero)]
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

    // Exportación a CSV (UTF-8 con BOM para que Excel respete las tildes).
    public async Task<IActionResult> VentasCsv(DateTime? desde, DateTime? hasta, int? idEmpleado)
    {
        if (desde > hasta)
        {
            return BadRequest("La fecha final no puede ser anterior a la inicial.");
        }

        var filas = await _reportes.ObtenerVentasAsync(desde, hasta, idEmpleado);
        var csv = new System.Text.StringBuilder("Factura,Fecha emision,Pedido,Mesa,Empleado,NIT,Total\r\n");
        foreach (var f in filas)
        {
            csv.Append(string.Join(",", f.NumFact, f.FechaEmision.ToString("yyyy-MM-dd HH:mm"), f.IdPedido, f.NumMesa,
                CampoCsv(f.Empleado), CampoCsv(f.Nit), f.Total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))).Append("\r\n");
        }
        return ArchivoCsv(csv.ToString(), "ventas.csv");
    }

    public async Task<IActionResult> PlatosCsv(DateTime? desde, DateTime? hasta, int top = 10)
    {
        if (desde > hasta)
        {
            return BadRequest("La fecha final no puede ser anterior a la inicial.");
        }

        var filas = await _reportes.ObtenerPlatosMasVendidosAsync(desde, hasta, Math.Clamp(top, 1, 50));
        var csv = new System.Text.StringBuilder("Codigo,Plato,Precio,Unidades,Monto\r\n");
        foreach (var f in filas)
        {
            csv.Append(string.Join(",", f.CodPlato, CampoCsv(f.Plato),
                f.Precio.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), f.Unidades,
                f.Monto.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))).Append("\r\n");
        }
        return ArchivoCsv(csv.ToString(), "platos-mas-vendidos.csv");
    }

    private static string CampoCsv(string valor)
    {
        // Evita inyección de fórmulas en Excel y escapa comillas y comas.
        if (valor.Length > 0 && "=+-@".Contains(valor[0]))
        {
            valor = "'" + valor;
        }
        return valor.Contains(',') || valor.Contains('"') ? $"\"{valor.Replace("\"", "\"\"")}\"" : valor;
    }

    private FileContentResult ArchivoCsv(string contenido, string nombre) =>
        File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(contenido)).ToArray(),
            "text/csv; charset=utf-8", nombre);

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
