using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;

namespace PedidoHub.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class BitacoraController : Controller
{
    private const int PorPagina = 25;
    private readonly PedidoHubDbContext _context;

    public BitacoraController(PedidoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? usuario, string? entidad, int pagina = 1)
    {
        var consulta = _context.Bitacoras.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(usuario))
        {
            consulta = consulta.Where(b => b.Usuario == usuario);
        }
        if (!string.IsNullOrWhiteSpace(entidad))
        {
            consulta = consulta.Where(b => b.Entidad == entidad);
        }

        var total = await consulta.CountAsync();
        pagina = Math.Max(1, pagina);
        var filas = await consulta.OrderByDescending(b => b.IdBitacora)
            .Skip((pagina - 1) * PorPagina).Take(PorPagina).ToListAsync();

        ViewData["Usuario"] = usuario;
        ViewData["Entidad"] = entidad;
        ViewData["Pagina"] = pagina;
        ViewData["TotalPaginas"] = Math.Max(1, (int)Math.Ceiling(total / (double)PorPagina));
        ViewData["Total"] = total;
        ViewData["Entidades"] = await _context.Bitacoras.AsNoTracking().Select(b => b.Entidad).Distinct().OrderBy(e => e).ToListAsync();
        return View(filas);
    }
}