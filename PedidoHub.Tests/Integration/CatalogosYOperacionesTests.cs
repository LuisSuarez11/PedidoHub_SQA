using System.Net;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Models;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Integration;

/// <summary>
/// Pruebas de aceptación (Dado / Cuando / Entonces) de teléfonos, mesas, empleados, búsqueda,
/// panel de mesa y edición/eliminación de pedidos y reservas.
/// </summary>
public class CatalogosYOperacionesTests : PruebaIntegracion
{
    private Task<RespuestaWeb> Enviar(string urlPost, Dictionary<string, string> campos) =>
        EnviarFormularioAsync("/Platos/Create", urlPost, campos);

    // ---------------------------------------------------------------- Teléfonos

    [Fact]
    public async Task Dado_un_cliente_Cuando_se_agrega_un_telefono_nuevo_Entonces_se_guarda()
    {
        var respuesta = await Enviar("/Telefonos/Create", new() { ["IdCliente"] = "2", ["Numero"] = "60001111" });

        Assert.True(respuesta.EsRedireccion);
        Assert.Equal(2, await ConBaseDeDatosAsync(bd => bd.Telefonos.CountAsync(t => t.IdCliente == 2)));
    }

    [Fact]
    public async Task Dado_un_telefono_repetido_Cuando_se_agrega_al_mismo_cliente_Entonces_se_rechaza()
    {
        var respuesta = await Enviar("/Telefonos/Create", new() { ["IdCliente"] = "1", ["Numero"] = "71234567" });

        Assert.Contains("Este cliente ya tiene registrado ese número.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dado_un_telefono_con_formato_invalido_Cuando_se_agrega_Entonces_se_rechaza()
    {
        var respuesta = await Enviar("/Telefonos/Create", new() { ["IdCliente"] = "2", ["Numero"] = "abc" });

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Telefonos.CountAsync(t => t.IdCliente == 2)));
    }

    [Fact]
    public async Task Dado_un_cliente_con_un_solo_telefono_Cuando_se_intenta_eliminar_Entonces_se_conserva()
    {
        await Enviar("/Telefonos/Delete/3", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Telefonos.CountAsync(t => t.IdTelefono == 3)));
    }

    [Fact]
    public async Task Dado_un_cliente_con_dos_telefonos_Cuando_se_elimina_uno_Entonces_queda_el_otro()
    {
        await Enviar("/Telefonos/Delete/2", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Telefonos.CountAsync(t => t.IdCliente == 1)));
    }

    [Fact]
    public async Task Dado_un_telefono_Cuando_se_edita_Entonces_cambia_el_numero()
    {
        await Enviar("/Telefonos/Edit/3", new() { ["IdTelefono"] = "3", ["Numero"] = "70009999" });

        var telefono = await ConBaseDeDatosAsync(bd => bd.Telefonos.SingleAsync(t => t.IdTelefono == 3));
        Assert.Equal("70009999", telefono.Numero);
    }

    // ---------------------------------------------------------------- Mesas

    [Fact]
    public async Task Dada_una_mesa_nueva_valida_Cuando_se_crea_Entonces_se_agrega()
    {
        var respuesta = await Enviar("/Mesas/Create", new() { ["Capacidad"] = "10" });

        Assert.True(respuesta.EsRedireccion);
        Assert.Equal(6, await ConBaseDeDatosAsync(bd => bd.Mesas.CountAsync()));
    }

    [Fact]
    public async Task Dada_una_capacidad_mayor_a_20_Cuando_se_crea_la_mesa_Entonces_se_rechaza()
    {
        var respuesta = await Enviar("/Mesas/Create", new() { ["Capacidad"] = "21" });

        Assert.Contains("La capacidad debe estar entre 1 y 20 personas.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dada_una_mesa_con_pedidos_Cuando_se_intenta_eliminar_Entonces_se_conserva()
    {
        await Enviar("/Mesas/Delete/1", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Mesas.CountAsync(m => m.NumMesa == 1)));
    }

    [Fact]
    public async Task Dada_una_mesa_sin_registros_Cuando_se_elimina_Entonces_desaparece()
    {
        await Enviar("/Mesas/Create", new() { ["Capacidad"] = "3" });
        var nueva = await ConBaseDeDatosAsync(bd => bd.Mesas.MaxAsync(m => m.NumMesa));

        await Enviar($"/Mesas/Delete/{nueva}", new());

        Assert.Equal(5, await ConBaseDeDatosAsync(bd => bd.Mesas.CountAsync()));
    }

    [Fact]
    public async Task Dada_una_mesa_Cuando_se_edita_su_capacidad_Entonces_se_actualiza()
    {
        await Enviar("/Mesas/Edit/1", new() { ["NumMesa"] = "1", ["Capacidad"] = "3" });

        Assert.Equal(3, (await ConBaseDeDatosAsync(bd => bd.Mesas.SingleAsync(m => m.NumMesa == 1))).Capacidad);
    }

    // ---------------------------------------------------------------- Empleados

    [Fact]
    public async Task Dado_un_empleado_valido_Cuando_se_registra_Entonces_se_guarda()
    {
        var respuesta = await Enviar("/Empleados/Create", new() { ["Nombre"] = "Sofía Vega", ["Cargo"] = "Cocinera" });

        Assert.True(respuesta.EsRedireccion);
        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Empleados.CountAsync(e => e.Nombre == "Sofía Vega")));
    }

    [Fact]
    public async Task Dado_un_empleado_sin_cargo_Cuando_se_registra_Entonces_se_rechaza()
    {
        var respuesta = await Enviar("/Empleados/Create", new() { ["Nombre"] = "Sin Cargo", ["Cargo"] = "" });

        Assert.Contains("El cargo es obligatorio.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dado_un_empleado_con_pedidos_Cuando_se_intenta_eliminar_Entonces_se_conserva()
    {
        await Enviar("/Empleados/Delete/1", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Empleados.CountAsync(e => e.IdEmpleado == 1)));
    }

    [Fact]
    public async Task Dado_un_empleado_sin_pedidos_Cuando_se_elimina_Entonces_desaparece()
    {
        await Enviar("/Empleados/Delete/3", new());

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Empleados.CountAsync(e => e.IdEmpleado == 3)));
    }

    // ---------------------------------------------------------------- Búsqueda y panel

    [Fact]
    public async Task Dada_la_paleta_Cuando_se_busca_un_plato_Entonces_devuelve_el_resultado_en_JSON()
    {
        var json = await LeerAsync("/Buscar?q=Silp");

        Assert.Contains("Silpancho", json);
    }

    [Fact]
    public async Task Dada_la_paleta_Cuando_se_escribe_un_solo_caracter_Entonces_no_devuelve_resultados()
    {
        Assert.Equal("[]", (await LeerAsync("/Buscar?q=S")).Trim());
    }

    [Fact]
    public async Task Dada_una_mesa_ocupada_Cuando_se_abre_su_panel_Entonces_muestra_el_pedido()
    {
        var respuesta = await Cliente.GetAsync("/Mesas/Panel/5");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Salteña", WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Dada_una_mesa_inexistente_Cuando_se_abre_su_panel_Entonces_devuelve_NotFound()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Cliente.GetAsync("/Mesas/Panel/99")).StatusCode);
    }

    [Theory]
    [InlineData("/Clientes/Delete/2")]
    [InlineData("/Platos/Delete/6")]
    [InlineData("/Reservas/Delete/4")]
    [InlineData("/Pedidos/Delete/5")]
    [InlineData("/Facturas/Delete/1")]
    [InlineData("/Mesas/Delete/1")]
    [InlineData("/Empleados/Delete/3")]
    [InlineData("/Telefonos/Edit/1")]
    [InlineData("/Mesas/Edit/1")]
    [InlineData("/Pedidos/Edit/5")]
    [InlineData("/Reservas/Edit/4")]
    [InlineData("/Platos/Edit/1")]
    [InlineData("/Empleados/Edit/1")]
    [InlineData("/Clientes/Edit/1")]
    public async Task Pantalla_de_confirmacion_o_edicion_carga(string url)
    {
        Assert.Equal(HttpStatusCode.OK, (await Cliente.GetAsync(url)).StatusCode);
    }

    // ---------------------------------------------------------------- Pedidos

    [Fact]
    public async Task Dado_un_pedido_abierto_Cuando_se_edita_a_Servido_Entonces_se_actualiza()
    {
        await Enviar("/Pedidos/Edit/5", new()
        {
            ["IdPedido"] = "5", ["FechaHora"] = "2026-10-25T12:45", ["Estado"] = "Servido",
            ["NumMesa"] = "5", ["IdEmpleado"] = "1"
        });

        Assert.Equal(EstadoPedido.Servido, (await ConBaseDeDatosAsync(bd => bd.Pedidos.SingleAsync(p => p.IdPedido == 5))).Estado);
    }

    [Fact]
    public async Task Dado_un_pedido_Cuando_se_marca_Facturado_a_mano_Entonces_se_rechaza()
    {
        var respuesta = await Enviar("/Pedidos/Edit/5", new()
        {
            ["IdPedido"] = "5", ["FechaHora"] = "2026-10-25T12:45", ["Estado"] = "Facturado",
            ["NumMesa"] = "5", ["IdEmpleado"] = "1"
        });

        Assert.Contains("se asigna automáticamente al generar la factura", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dado_un_pedido_facturado_Cuando_se_intenta_editar_Entonces_se_bloquea()
    {
        var respuesta = await Enviar("/Pedidos/Edit/1", new()
        {
            ["IdPedido"] = "1", ["FechaHora"] = "2026-10-03T13:15", ["Estado"] = "Cancelado",
            ["NumMesa"] = "1", ["IdEmpleado"] = "1"
        });

        Assert.True(respuesta.EsRedireccion);
        Assert.Equal(EstadoPedido.Facturado, (await ConBaseDeDatosAsync(bd => bd.Pedidos.SingleAsync(p => p.IdPedido == 1))).Estado);
    }

    [Fact]
    public async Task Dado_un_pedido_sin_factura_Cuando_se_elimina_Entonces_desaparece_con_sus_platos()
    {
        await Enviar("/Pedidos/Delete/5", new());

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Pedidos.CountAsync(p => p.IdPedido == 5)));
        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.PedidoPlatos.CountAsync(d => d.IdPedido == 5)));
    }

    [Fact]
    public async Task Dado_un_plato_en_un_pedido_abierto_Cuando_se_quita_Entonces_ya_no_figura()
    {
        await Enviar("/Pedidos/QuitarPlato", new() { ["idPedido"] = "5", ["codPlato"] = "4" });

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.PedidoPlatos.CountAsync(d => d.IdPedido == 5 && d.CodPlato == 4)));
    }

    [Fact]
    public async Task Dada_una_cantidad_fuera_de_rango_Cuando_se_agrega_un_plato_Entonces_se_rechaza()
    {
        await Enviar("/Pedidos/AgregarPlato",
            new() { ["NuevoPlato.IdPedido"] = "5", ["NuevoPlato.CodPlato"] = "6", ["NuevoPlato.Cantidad"] = "101" });

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.PedidoPlatos.CountAsync(d => d.IdPedido == 5 && d.CodPlato == 6)));
    }

    // ---------------------------------------------------------------- Reservas

    [Fact]
    public async Task Dada_una_reserva_pendiente_Cuando_se_cancela_Entonces_cambia_su_estado()
    {
        await Enviar("/Reservas/Edit/4", new()
        {
            ["IdReserva"] = "4", ["FechaHora"] = "2026-11-02T12:30", ["CantPersonas"] = "3",
            ["Estado"] = "Cancelada", ["IdCliente"] = "4", ["NumMesa"] = "3"
        });

        Assert.Equal(EstadoReserva.Cancelada, (await ConBaseDeDatosAsync(bd => bd.Reservas.SingleAsync(r => r.IdReserva == 4))).Estado);
    }

    [Fact]
    public async Task Dada_una_reserva_Cuando_se_eliminan_Entonces_desaparece()
    {
        await Enviar("/Reservas/Delete/4", new());

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Reservas.CountAsync(r => r.IdReserva == 4)));
    }

    [Fact]
    public async Task Dada_una_reserva_existente_Cuando_se_edita_con_mas_personas_que_la_capacidad_Entonces_se_rechaza()
    {
        var respuesta = await Enviar("/Reservas/Edit/4", new()
        {
            ["IdReserva"] = "4", ["FechaHora"] = "2026-11-02T12:30", ["CantPersonas"] = "9",
            ["Estado"] = "Pendiente", ["IdCliente"] = "4", ["NumMesa"] = "3"
        });

        Assert.Contains("La mesa 3 tiene capacidad para 4 personas.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dado_un_filtro_por_estado_Cuando_se_lista_Entonces_solo_muestra_ese_estado()
    {
        var html = await LeerAsync("/Reservas?estado=Completada");

        Assert.Contains("Ana Torres", html);
        Assert.DoesNotContain("Diego Ruiz", html);
    }

    [Fact]
    public async Task Dado_un_rango_de_fechas_Cuando_se_listan_pedidos_Entonces_filtra_por_fecha()
    {
        var html = await LeerAsync("/Pedidos?desde=2026-10-24&hasta=2026-10-25");

        Assert.Contains("Mesa 5", html);
    }
}
