using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace PedidoHub.E2E.Infra;

/// <summary>
/// Base de las pruebas de extremo a extremo con Selenium WebDriver.
/// Requiere la aplicación en ejecución en <c>PEDIDOHUB_URL</c> (por defecto http://localhost:5158).
/// Usa Chrome sin interfaz gráfica (headless); defina <c>E2E_VISIBLE=1</c> para verlo en pantalla.
/// </summary>
public abstract class PruebaNavegador : IDisposable
{
    protected static readonly string UrlBase =
        (Environment.GetEnvironmentVariable("PEDIDOHUB_URL") ?? "http://localhost:5158").TrimEnd('/');

    protected IWebDriver Navegador { get; }
    protected WebDriverWait Espera { get; }

    protected static readonly string Clave =
        Environment.GetEnvironmentVariable("PEDIDOHUB_CLAVE") ?? "PedidoHub2026!";

    protected PruebaNavegador(string? usuario = "admin")
    {
        var opciones = new ChromeOptions();
        if (Environment.GetEnvironmentVariable("E2E_VISIBLE") != "1")
        {
            opciones.AddArgument("--headless=new");
        }
        opciones.AddArguments("--window-size=1440,900", "--no-sandbox", "--disable-dev-shm-usage", "--lang=es");

        Navegador = new ChromeDriver(opciones);
        Navegador.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        Espera = new WebDriverWait(Navegador, TimeSpan.FromSeconds(10));

        if (usuario is not null)
        {
            IniciarSesion(usuario);
        }
    }

    protected void IniciarSesion(string usuario, string? clave = null)
    {
        Ir("/Cuenta/Ingresar");
        Escribir("NombreUsuario", usuario);
        Escribir("Clave", clave ?? Clave);
        Navegador.FindElement(By.CssSelector(".login-caja button[type=submit]")).Click();
    }

    protected void Ir(string ruta) => Navegador.Navigate().GoToUrl(UrlBase + ruta);

    protected IWebElement Campo(string nombre) => Navegador.FindElement(By.Name(nombre));

    protected void Escribir(string nombre, string texto)
    {
        var campo = Campo(nombre);
        campo.Clear();
        campo.SendKeys(texto);
    }

    protected void Elegir(string nombre, string textoVisible) =>
        new SelectElement(Campo(nombre)).SelectByText(textoVisible);

    /// <summary>Los inputs datetime-local no aceptan bien SendKeys: se fija el valor y se dispara "change".</summary>
    protected void FijarFechaHora(string nombre, DateTime valor)
    {
        ((IJavaScriptExecutor)Navegador).ExecuteScript(
            "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
            Campo(nombre), valor.ToString("yyyy-MM-ddTHH:mm"));
    }

    protected void Guardar() =>
        Navegador.FindElement(By.CssSelector("form.form-card button[type=submit], form button.btn-primary[type=submit]")).Click();

    protected string TextoDeAvisos() =>
        string.Join(" ", Navegador.FindElements(By.CssSelector(".toast-ph")).Select(t => t.Text));

    protected static string Unico(string prefijo) => $"{prefijo} {Guid.NewGuid().ToString("N")[..6]}";

    public void Dispose()
    {
        Navegador.Quit();
        Navegador.Dispose();
        GC.SuppressFinalize(this);
    }
}
