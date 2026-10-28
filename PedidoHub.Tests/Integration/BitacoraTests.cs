using Microsoft.EntityFrameworkCore;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Integration;

/// <summary>Trazabilidad: cada alta, cambio o baja queda registrada con su usuario (HU-14).</summary>
public class BitacoraTests : PruebaIntegracion
{
    [Fact]
    public async Task Dado_un_plato_nuevo_Cuando_se_crea_Entonces_la_bitacora_registra_quien_lo_creo()
    {
        await EnviarFormularioAsync("/Platos/Create", "/Platos/Create", new() { ["Nombre"] = "Majadito", ["Precio"] = "30" });

        var registro = await ConBaseDeDatosAsync(bd => bd.Bitacoras.SingleAsync(b => b.Entidad == "Plato"));
        Assert.Equal(("admin", "Creó"), (registro.Usuario, registro.Accion));
    }

    [Fact]
    public async Task Dada_una_mesa_Cuando_se_modifica_Entonces_el_detalle_muestra_el_valor_anterior_y_el_nuevo()
    {
        await EnviarFormularioAsync("/Mesas/Edit/1", "/Mesas/Edit/1", new() { ["NumMesa"] = "1", ["Capacidad"] = "3" });

        var registro = await ConBaseDeDatosAsync(bd => bd.Bitacoras.SingleAsync(b => b.Entidad == "Mesa" && b.Accion == "Modificó"));
        Assert.Contains("Capacidad: 2 → 3", registro.Detalle);
        Assert.Equal("1", registro.Registro);
    }

    [Fact]
    public async Task Dado_un_empleado_sin_pedidos_Cuando_se_elimina_Entonces_queda_registrada_la_baja()
    {
        await EnviarFormularioAsync("/Empleados/Delete/3", "/Empleados/Delete/3", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Bitacoras.CountAsync(b => b.Entidad == "Empleado" && b.Accion == "Eliminó")));
    }

    [Fact]
    public async Task Dada_una_operacion_de_un_cajero_Cuando_factura_Entonces_la_bitacora_guarda_su_usuario()
    {
        var cajero = await ClienteDeAsync("cajero");
        var pagina = await cajero.GetStringAsync("/Facturas/Create?idPedido=5");
        var token = System.Text.RegularExpressions.Regex.Match(pagina, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

        await cajero.PostAsync("/Facturas/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["IdPedido"] = "5", ["NIT"] = "5566778899", ["FechaEmision"] = "2026-10-26T13:00", ["__RequestVerificationToken"] = token
        }));

        var usuarios = await ConBaseDeDatosAsync(bd => bd.Bitacoras.Where(b => b.Entidad == "Factura").Select(b => b.Usuario).ToListAsync());
        Assert.Equal(new[] { "cajero" }, usuarios);
    }

    [Fact]
    public async Task Dados_los_datos_iniciales_Cuando_arranca_el_sistema_Entonces_la_bitacora_esta_vacia()
    {
        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Bitacoras.CountAsync()));
    }

    [Fact]
    public async Task Dado_un_cambio_Cuando_el_administrador_abre_la_bitacora_Entonces_lo_ve_en_la_tabla_y_puede_filtrarlo()
    {
        await EnviarFormularioAsync("/Platos/Create", "/Platos/Create", new() { ["Nombre"] = "Chicharrón", ["Precio"] = "45" });

        var html = await LeerAsync("/Bitacora?entidad=Plato");

        Assert.Contains("Plato", html);
        Assert.Contains("Creó", html);
        Assert.DoesNotContain("Mesa", html.Split("<tbody>")[1].Split("</tbody>")[0]);
    }

    [Fact]
    public async Task Dada_la_bitacora_Cuando_no_hay_coincidencias_Entonces_informa_que_esta_vacia()
    {
        var html = await LeerAsync("/Bitacora?usuario=nadie");

        Assert.Contains("Todavía no hay movimientos registrados.", html);
    }
}