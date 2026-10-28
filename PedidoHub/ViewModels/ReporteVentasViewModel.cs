using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PedidoHub.ViewModels;

public record VentaFila(int NumFact, DateTime FechaEmision, int IdPedido, int NumMesa, string Empleado, string Nit, decimal Total);

public class ReporteVentasViewModel
{
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    [Display(Name = "Empleado")]
    public int? IdEmpleado { get; set; }

    public IEnumerable<SelectListItem> Empleados { get; set; } = new List<SelectListItem>();
    public List<VentaFila> Filas { get; set; } = new();

    public int CantidadFacturas => Filas.Count;
    public decimal TotalFacturado => Filas.Sum(f => f.Total);
    public decimal TicketPromedio => Filas.Count == 0 ? 0 : TotalFacturado / Filas.Count;
}
