using System.ComponentModel.DataAnnotations;
using PedidoHub.Models;
using PedidoHub.ViewModels;

namespace PedidoHub.Tests.Unit;

/// <summary>Pruebas unitarias de las reglas de validación declaradas en los modelos (Data Annotations).</summary>
public class ValidacionModelosTests
{
    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }

    [Theory]
    [InlineData("12345", true)]
    [InlineData("1020304011", true)]
    [InlineData("12345678901234567890", true)]
    [InlineData("1234", false)]
    [InlineData("123456789012345678901", false)]
    [InlineData("ABC12345", false)]
    [InlineData("1234-5678", false)]
    [InlineData("", false)]
    public void Factura_Nit_SoloAceptaDigitosEntre5Y20(string nit, bool esperadoValido)
    {
        var modelo = new FacturaFormViewModel { NIT = nit, FechaEmision = DateTime.Now };

        Assert.Equal(esperadoValido, Validar(modelo).Count == 0);
    }

    [Theory]
    [InlineData("71234567", true)]
    [InlineData("+591 71234567", true)]
    [InlineData("2-123456", true)]
    [InlineData("123456", false)]
    [InlineData("telefono", false)]
    [InlineData("71234567; DROP TABLE", false)]
    public void Telefono_Numero_ValidaFormato(string numero, bool esperadoValido)
    {
        var telefono = new TelefonoCliente { Numero = numero };

        Assert.Equal(esperadoValido, Validar(telefono).Count == 0);
    }

    [Theory]
    [InlineData(0.01, true)]
    [InlineData(55, true)]
    [InlineData(10000, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(10000.01, false)]
    public void Plato_Precio_DebeEstarEntreCentavoYDiezMil(double precio, bool esperadoValido)
    {
        var plato = new Plato { Nombre = "Prueba", Precio = (decimal)precio };

        Assert.Equal(esperadoValido, Validar(plato).Count == 0);
    }

    [Fact]
    public void Plato_NombreVacio_EsInvalido()
    {
        var errores = Validar(new Plato { Nombre = "", Precio = 10 });

        Assert.Contains(errores, e => e.ErrorMessage == "El nombre del plato es obligatorio.");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(20, true)]
    [InlineData(0, false)]
    [InlineData(21, false)]
    public void Mesa_Capacidad_DebeEstarEntre1Y20(int capacidad, bool esperadoValido)
    {
        Assert.Equal(esperadoValido, Validar(new Mesa { Capacidad = capacidad }).Count == 0);
    }

    [Fact]
    public void Cliente_NombreMayorA100Caracteres_EsInvalido()
    {
        var errores = Validar(new Cliente { Nombre = new string('a', 101) });

        Assert.Single(errores);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(101, false)]
    public void AgregarPlato_Cantidad_DebeEstarEntre1Y100(int cantidad, bool esperadoValido)
    {
        var modelo = new AgregarPlatoViewModel { IdPedido = 1, CodPlato = 1, Cantidad = cantidad };

        Assert.Equal(esperadoValido, Validar(modelo).Count == 0);
    }

    [Fact]
    public void Empleado_SinCargo_EsInvalido()
    {
        var errores = Validar(new Empleado { Nombre = "Laura", Cargo = "" });

        Assert.Contains(errores, e => e.ErrorMessage == "El cargo es obligatorio.");
    }
}
