using OpenQA.Selenium;
using PedidoHub.E2E.Infra;

namespace PedidoHub.E2E;

/// <summary>Landing pública, inicio de sesión y permisos por rol (HU-13, HU-15, HU-16).</summary>
public class AccesoYLandingTests : PruebaNavegador
{
    public AccesoYLandingTests(Xunit.Abstractions.ITestOutputHelper salida) : base(salida, usuario: null)
    {
    }

    [Fact]
    public void E2E_12_Dado_un_visitante_Cuando_abre_la_raiz_Entonces_ve_la_landing_con_indicadores_de_calidad()
    {
        Ir("/");

        var texto = Navegador.FindElement(By.TagName("main")).Text;
        Assert.Contains("calidad medible", texto);
        Assert.Contains("pruebas automatizadas", texto);
        Assert.Contains("cobertura de líneas", texto);
    }

    [Fact]
    public void E2E_13_Dado_un_visitante_Cuando_abre_una_pantalla_interna_Entonces_es_enviado_al_login()
    {
        Ir("/Clientes");

        Espera.Until(d => d.Url.Contains("/Cuenta/Ingresar"));
    }

    [Fact]
    public void E2E_14_Dada_una_clave_incorrecta_Cuando_se_ingresa_Entonces_muestra_el_error_y_no_entra()
    {
        IniciarSesion("admin", "clave-incorrecta");

        Espera.Until(d => d.FindElement(By.TagName("main")).Text.Contains("Usuario o contraseña incorrectos."));
        Assert.Contains("/Cuenta/Ingresar", Navegador.Url);
    }

    [Fact]
    public void E2E_15_Dadas_credenciales_de_administrador_Cuando_se_ingresa_Entonces_ve_el_panel_de_calidad()
    {
        IniciarSesion("admin");
        Espera.Until(d => d.Url.Contains("/Home/Servicio"));

        Ir("/Calidad");

        var texto = Navegador.FindElement(By.TagName("main")).Text;
        Assert.Contains("Panel de calidad", texto);
        Assert.Contains("Definition of Done", texto);
    }

    [Fact]
    public void E2E_16_Dado_un_mesero_Cuando_abre_Empleados_Entonces_ve_acceso_denegado()
    {
        IniciarSesion("mesero");
        Espera.Until(d => d.Url.Contains("/Home/Servicio"));

        Ir("/Empleados");

        Espera.Until(d => d.Url.Contains("/Cuenta/Denegado"));
        Assert.Contains("Acceso denegado", Navegador.FindElement(By.TagName("main")).Text);
    }

    [Fact]
    public void E2E_17_Dada_una_sesion_activa_Cuando_se_cierra_Entonces_vuelve_a_pedir_login()
    {
        IniciarSesion("cajero");
        Espera.Until(d => d.Url.Contains("/Home/Servicio"));

        Navegador.FindElement(By.CssSelector(".usuario-caja button[type=submit]")).Click();
        Espera.Until(d => d.FindElement(By.TagName("main")).Text.Contains("calidad medible"));
        Ir("/Clientes");

        Espera.Until(d => d.Url.Contains("/Cuenta/Ingresar"));
    }
}