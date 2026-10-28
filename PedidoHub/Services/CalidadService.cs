using System.Text.Json;
using PedidoHub.ViewModels;

namespace PedidoHub.Services;

/// <summary>Lee las métricas de calidad del sprint (Data/calidad.json), que actualiza scripts/cobertura.ps1.</summary>
public class CalidadService
{
    private readonly IWebHostEnvironment _entorno;

    public CalidadService(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public DatosCalidad Leer()
    {
        var archivo = _entorno.ContentRootFileProvider.GetFileInfo("Data/calidad.json");
        if (!archivo.Exists)
        {
            return new DatosCalidad();
        }

        using var lector = new StreamReader(archivo.CreateReadStream());
        return JsonSerializer.Deserialize<DatosCalidad>(lector.ReadToEnd()) ?? new DatosCalidad();
    }
}