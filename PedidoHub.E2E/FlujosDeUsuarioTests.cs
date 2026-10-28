using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PedidoHub.E2E.Infra;

namespace PedidoHub.E2E;

/// <summary>
/// Pruebas de aceptación de extremo a extremo (Selenium) sobre los flujos críticos de PedidoHub.
/// Cada escenario se redacta como Dado / Cuando / Entonces y usa datos únicos para poder repetirse.
/// </summary>
public class FlujosDeUsuarioTests : PruebaNavegador
{
    [Fact]
    public void E2E_01_Dado_el_inicio_Cuando_se_abre_Entonces_muestra_el_plano_con_las_mesas()
    {
        Ir("/Home/Servicio");

        Assert.Contains("PedidoHub", Navegador.Title);
        var texto = Navegador.FindElement(By.TagName("main")).Text;
        Assert.Contains("Mesa 1", texto);
        Assert.Contains("Mesa 5", texto);
    }

    [Fact]
    public void E2E_02_Dada_la_paleta_Cuando_se_busca_un_plato_Entonces_aparece_en_los_resultados()
    {
        Ir("/Home/Servicio");

        Navegador.FindElement(By.TagName("body")).SendKeys(Keys.Control + "k");
        var entrada = Espera.Until(d => d.FindElement(By.Id("paleta-entrada")));
        entrada.SendKeys("Silpancho");

        Espera.Until(d => d.FindElement(By.Id("paleta-lista")).Text.Contains("Silpancho"));
    }

    [Fact]
    public void E2E_03_Dado_un_cliente_valido_Cuando_se_registra_Entonces_se_muestra_el_aviso_de_exito()
    {
        var nombre = Unico("Cliente QA");
        Ir("/Clientes/Create");

        Escribir("Nombre", nombre);
        Escribir("Telefono", "71234500");
        Guardar();

        Espera.Until(d => TextoDeAvisos().Contains("registrado"));
        Ir("/Clientes");
        Assert.Contains(nombre, Navegador.FindElement(By.TagName("main")).Text);
    }

    [Fact]
    public void E2E_04_Dado_un_cliente_sin_telefono_Cuando_se_guarda_Entonces_se_muestra_el_error_de_validacion()
    {
        Ir("/Clientes/Create");

        Escribir("Nombre", Unico("Sin telefono"));
        Guardar();

        Espera.Until(d => d.FindElements(By.CssSelector(".text-danger")).Any(e => e.Text.Length > 0));
        Assert.EndsWith("/Clientes/Create", Navegador.Url);
    }

    [Fact]
    public void E2E_05_Dado_un_plato_nuevo_Cuando_se_crea_Entonces_aparece_en_el_menu_con_su_precio()
    {
        var nombre = Unico("Plato QA");
        Ir("/Platos/Create");

        Escribir("Nombre", nombre);
        Escribir("Precio", "33.50");
        Guardar();

        Espera.Until(d => d.FindElement(By.TagName("main")).Text.Contains(nombre));
        Assert.Contains("33", Navegador.FindElement(By.TagName("main")).Text);
    }

    [Fact]
    public void E2E_06_Dada_una_mesa_de_4_Cuando_se_reservan_5_personas_Entonces_se_rechaza_por_capacidad()
    {
        Ir("/Reservas/Create");

        Elegir("IdCliente", "Ana Torres");
        Elegir("NumMesa", "Mesa 2 (capacidad 4)");
        FijarFechaHora("FechaHora", DateTime.Today.AddDays(200).AddHours(20));
        Escribir("CantPersonas", "5");
        Guardar();

        Espera.Until(d => d.FindElement(By.TagName("main")).Text.Contains("capacidad para 4 personas"));
    }

    [Fact]
    public void E2E_07_Dada_una_fecha_pasada_Cuando_se_crea_una_reserva_Entonces_se_rechaza()
    {
        Ir("/Reservas/Create");

        Elegir("IdCliente", "Ana Torres");
        Elegir("NumMesa", "Mesa 2 (capacidad 4)");
        FijarFechaHora("FechaHora", new DateTime(2026, 1, 10, 20, 0, 0));
        Escribir("CantPersonas", "2");
        Guardar();

        Espera.Until(d => d.FindElement(By.TagName("main")).Text.Contains("no puede estar en el pasado"));
    }

    [Fact]
    public void E2E_08_Dado_el_reporte_de_ventas_Cuando_se_filtra_por_fechas_invertidas_Entonces_informa_el_error()
    {
        Ir("/Reportes/Ventas?Desde=2026-10-20&Hasta=2026-10-01");

        Assert.Contains("La fecha final no puede ser anterior a la inicial.",
            Navegador.FindElement(By.TagName("main")).Text);
    }

    [Fact]
    public void E2E_09_Dado_el_reporte_de_platos_Cuando_se_abre_Entonces_lista_los_platos_vendidos()
    {
        Ir("/Reportes/PlatosMasVendidos");

        var texto = Navegador.FindElement(By.TagName("main")).Text;
        Assert.Contains("Refresco", texto);
    }

    [Fact]
    public void E2E_10_Dada_una_ruta_inexistente_Cuando_se_visita_Entonces_no_se_expone_informacion_tecnica()
    {
        Ir("/Pedidos/Details/999999");

        var cuerpo = Navegador.FindElement(By.TagName("body")).Text;
        Assert.DoesNotContain("Exception", cuerpo);
        Assert.DoesNotContain("StackTrace", cuerpo);
    }

    [Fact]
    public void E2E_11_Dada_una_pantalla_movil_Cuando_se_abre_el_inicio_Entonces_el_menu_es_accesible_por_boton()
    {
        Navegador.Manage().Window.Size = new System.Drawing.Size(390, 844);
        Ir("/Home/Servicio");

        var boton = Navegador.FindElement(By.CssSelector("[data-bs-target='#menuLateral']"));
        Assert.True(boton.Displayed);
    }
}
