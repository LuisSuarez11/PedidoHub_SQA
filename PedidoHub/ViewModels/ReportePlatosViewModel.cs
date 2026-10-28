using System.ComponentModel.DataAnnotations;

namespace PedidoHub.ViewModels;

public record PlatoVendidoFila(int CodPlato, string Plato, decimal Precio, int Unidades, decimal Monto);

public class ReportePlatosViewModel
{
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    [Display(Name = "Mostrar los primeros")]
    [Range(1, 100, ErrorMessage = "Indique un número entre 1 y 100.")]
    public int Top { get; set; } = 10;

    public List<PlatoVendidoFila> Filas { get; set; } = new();

    public int TotalUnidades => Filas.Sum(f => f.Unidades);
    public decimal TotalMonto => Filas.Sum(f => f.Monto);
}
