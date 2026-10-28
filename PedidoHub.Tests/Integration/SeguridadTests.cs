using System.Net;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Integration;

/// <summary>Autenticación, autorización por rol y protección de la sesión (HU-13).</summary>
public class SeguridadTests : PruebaIntegracion
{
    private HttpClient Anonimo() => CrearCliente();

    [Fact]
    public async Task Dado_un_visitante_sin_sesion_Cuando_abre_una_pantalla_interna_Entonces_va_al_login()
    {
        var respuesta = await Anonimo().GetAsync("/Clientes");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Contains("/Cuenta/Ingresar", respuesta.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Dado_un_visitante_Cuando_abre_la_pagina_principal_Entonces_ve_la_landing_publica()
    {
        var respuesta = await Anonimo().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("calidad medible", WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Dado_un_visitante_Cuando_consulta_health_Entonces_no_necesita_sesion()
    {
        Assert.Equal(HttpStatusCode.OK, (await Anonimo().GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task Dada_una_clave_incorrecta_Cuando_se_ingresa_Entonces_se_rechaza_con_mensaje_generico()
    {
        var cliente = Anonimo();

        var respuesta = await IniciarSesionAsync(cliente, "admin", "clave-equivocada");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Usuario o contraseña incorrectos.", WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync()));
        Assert.Equal(HttpStatusCode.Redirect, (await cliente.GetAsync("/Clientes")).StatusCode);
    }

    [Fact]
    public async Task Dado_un_usuario_inexistente_Cuando_se_ingresa_Entonces_el_mensaje_no_revela_si_existe()
    {
        var respuesta = await IniciarSesionAsync(Anonimo(), "fantasma", "PedidoHub2026!");

        Assert.Contains("Usuario o contraseña incorrectos.", WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Dadas_credenciales_correctas_Cuando_se_ingresa_Entonces_entra_al_servicio()
    {
        var respuesta = await IniciarSesionAsync(Anonimo(), "cajero");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Home/Servicio", respuesta.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Dada_una_direccion_de_retorno_externa_Cuando_se_ingresa_Entonces_no_hay_redireccion_abierta()
    {
        var cliente = Anonimo();
        var pagina = await cliente.GetStringAsync("/Cuenta/Ingresar");
        var token = System.Text.RegularExpressions.Regex.Match(pagina, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

        var respuesta = await cliente.PostAsync("/Cuenta/Ingresar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["NombreUsuario"] = "admin", ["Clave"] = ClaveDemo, ["ReturnUrl"] = "https://sitio-malicioso.example", ["__RequestVerificationToken"] = token
        }));

        Assert.DoesNotContain("malicioso", respuesta.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("/Empleados")]
    [InlineData("/Calidad")]
    [InlineData("/Bitacora")]
    [InlineData("/Facturas")]
    [InlineData("/Reportes/Ventas")]
    public async Task Dado_un_mesero_Cuando_abre_una_pantalla_restringida_Entonces_se_le_deniega(string url)
    {
        var mesero = await ClienteDeAsync("mesero");

        var respuesta = await mesero.GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Contains("/Cuenta/Denegado", respuesta.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("/Home/Servicio")]
    [InlineData("/Pedidos")]
    [InlineData("/Reservas")]
    [InlineData("/Clientes")]
    public async Task Dado_un_mesero_Cuando_abre_las_pantallas_de_servicio_Entonces_puede_verlas(string url)
    {
        var mesero = await ClienteDeAsync("mesero");

        Assert.Equal(HttpStatusCode.OK, (await mesero.GetAsync(url)).StatusCode);
    }

    [Theory]
    [InlineData("/Facturas", HttpStatusCode.OK)]
    [InlineData("/Reportes/Ventas", HttpStatusCode.OK)]
    [InlineData("/Empleados", HttpStatusCode.Redirect)]
    [InlineData("/Calidad", HttpStatusCode.Redirect)]
    public async Task Dado_un_cajero_Cuando_abre_pantallas_Entonces_se_respeta_su_rol(string url, HttpStatusCode esperado)
    {
        var cajero = await ClienteDeAsync("cajero");

        Assert.Equal(esperado, (await cajero.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Dado_el_administrador_Cuando_abre_el_panel_de_calidad_Entonces_ve_las_metricas()
    {
        var html = await LeerAsync("/Calidad");

        Assert.Contains("Panel de calidad", html);
        Assert.Contains("Estado vivo del sistema", html);
        Assert.Contains("Definition of Done", html);
    }

    [Fact]
    public async Task Dada_una_sesion_activa_Cuando_se_cierra_Entonces_ya_no_accede()
    {
        await EnviarFormularioAsync("/Platos/Create", "/Cuenta/Salir", new());

        Assert.Equal(HttpStatusCode.Redirect, (await Cliente.GetAsync("/Clientes")).StatusCode);
    }

    [Fact]
    public async Task Dadas_las_claves_almacenadas_Entonces_estan_hasheadas_y_no_en_texto_plano()
    {
        var claves = await ConBaseDeDatosAsync(bd => bd.Usuarios.Select(u => u.ClaveHash).ToListAsync());

        Assert.Equal(3, claves.Count);
        Assert.All(claves, c => Assert.DoesNotContain(ClaveDemo, c));
        Assert.All(claves, c => Assert.StartsWith("AQAAAA", c));
    }

    [Fact]
    public async Task Dada_la_cookie_de_sesion_Entonces_es_HttpOnly()
    {
        var cliente = Anonimo();

        var respuesta = await IniciarSesionAsync(cliente, "admin");

        var cookie = respuesta.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("PedidoHub.Sesion"));
        Assert.Contains("httponly", cookie.ToLowerInvariant());
    }
}