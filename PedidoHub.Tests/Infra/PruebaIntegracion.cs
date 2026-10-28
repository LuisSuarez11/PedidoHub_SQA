using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PedidoHub.Data;

namespace PedidoHub.Tests.Infra;

/// <summary>
/// Base de las pruebas de integración: una aplicación nueva (con sus datos de ejemplo) por prueba
/// y utilidades para navegar y enviar formularios como lo haría un navegador (cookies + antiforgery).
/// </summary>
public abstract class PruebaIntegracion : IDisposable
{
    private static readonly Regex TokenRegex =
        new("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.Compiled);

    protected PedidoHubFactory Fabrica { get; } = new();
    protected HttpClient Cliente { get; }

    protected PruebaIntegracion()
    {
        Cliente = Fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    protected async Task<string> LeerAsync(string url)
    {
        var respuesta = await Cliente.GetAsync(url);
        return WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>Envía un formulario POST obteniendo antes el token antiforgery de <paramref name="urlFormulario"/>.</summary>
    protected async Task<RespuestaWeb> EnviarFormularioAsync(
        string urlFormulario, string urlPost, Dictionary<string, string> campos)
    {
        var pagina = await Cliente.GetStringAsync(urlFormulario);
        var coincidencia = TokenRegex.Match(pagina);
        Assert.True(coincidencia.Success, $"La página {urlFormulario} no tiene token antiforgery.");

        campos["__RequestVerificationToken"] = coincidencia.Groups[1].Value;
        var respuesta = await Cliente.PostAsync(urlPost, new FormUrlEncodedContent(campos));
        var cuerpo = WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
        return new RespuestaWeb(respuesta.StatusCode, respuesta.Headers.Location?.OriginalString, cuerpo);
    }

    /// <summary>Ejecuta código directamente contra la base de datos de la aplicación en prueba.</summary>
    protected async Task<T> ConBaseDeDatosAsync<T>(Func<PedidoHubDbContext, Task<T>> accion)
    {
        using var alcance = Fabrica.Services.CreateScope();
        return await accion(alcance.ServiceProvider.GetRequiredService<PedidoHubDbContext>());
    }

    public void Dispose()
    {
        Cliente.Dispose();
        Fabrica.Dispose();
        GC.SuppressFinalize(this);
    }
}

public record RespuestaWeb(HttpStatusCode Estado, string? Redireccion, string Cuerpo)
{
    public bool EsRedireccion => (int)Estado is >= 300 and < 400;
}
