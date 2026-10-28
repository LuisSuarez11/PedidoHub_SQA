using System.Net;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Models;
using PedidoHub.Tests.Infra;

namespace PedidoHub.Tests.Integration;

/// <summary>
/// Pruebas de aceptación (estilo BDD: Dado / Cuando / Entonces) de las reglas de negocio,
/// ejecutadas contra la aplicación completa (controladores, vistas, EF Core y SQLite).
/// </summary>
public class ReglasDeNegocioTests : PruebaIntegracion
{
    private static string FechaFutura(int dias = 60, int hora = 20) =>
        DateTime.Today.AddDays(dias).AddHours(hora).ToString("yyyy-MM-ddTHH:mm");

    private Task<RespuestaWeb> CrearReserva(int cliente, int mesa, int personas, string fechaHora, string estado = "Pendiente") =>
        EnviarFormularioAsync("/Reservas/Create", "/Reservas/Create", new()
        {
            ["FechaHora"] = fechaHora,
            ["CantPersonas"] = personas.ToString(),
            ["Estado"] = estado,
            ["IdCliente"] = cliente.ToString(),
            ["NumMesa"] = mesa.ToString()
        });

    // ---------------------------------------------------------------- Clientes

    [Fact]
    public async Task Dado_un_cliente_nuevo_valido_Cuando_se_registra_Entonces_se_guarda_con_su_primer_telefono()
    {
        var respuesta = await EnviarFormularioAsync("/Clientes/Create", "/Clientes/Create",
            new() { ["Nombre"] = "  Elena Paz  ", ["Telefono"] = "72223344" });

        Assert.True(respuesta.EsRedireccion);
        var cliente = await ConBaseDeDatosAsync(bd =>
            bd.Clientes.Include(c => c.Telefonos).SingleAsync(c => c.Nombre == "Elena Paz"));
        Assert.Equal("72223344", Assert.Single(cliente.Telefonos).Numero);
    }

    [Fact]
    public async Task Dado_un_cliente_sin_telefono_Cuando_se_registra_Entonces_se_rechaza()
    {
        var respuesta = await EnviarFormularioAsync("/Clientes/Create", "/Clientes/Create",
            new() { ["Nombre"] = "Sin Telefono", ["Telefono"] = "" });

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Clientes.CountAsync(c => c.Nombre == "Sin Telefono")));
    }

    [Fact]
    public async Task Dado_un_cliente_con_reservas_Cuando_se_intenta_eliminar_Entonces_se_conserva()
    {
        await EnviarFormularioAsync("/Clientes/Delete/1", "/Clientes/Delete/1", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Clientes.CountAsync(c => c.IdCliente == 1)));
    }

    // ---------------------------------------------------------------- Platos

    [Fact]
    public async Task Dado_un_plato_existente_Cuando_se_crea_otro_con_el_mismo_nombre_Entonces_se_rechaza()
    {
        var respuesta = await EnviarFormularioAsync("/Platos/Create", "/Platos/Create",
            new() { ["Nombre"] = "Silpancho", ["Precio"] = "30" });

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Contains("Ya existe un plato con ese nombre.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dado_un_plato_con_precio_cero_Cuando_se_crea_Entonces_se_rechaza()
    {
        var respuesta = await EnviarFormularioAsync("/Platos/Create", "/Platos/Create",
            new() { ["Nombre"] = "Gratis", ["Precio"] = "0" });

        Assert.Contains("El precio debe estar entre 0.01 y 10000.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dado_un_plato_usado_en_pedidos_Cuando_se_intenta_eliminar_Entonces_se_conserva()
    {
        await EnviarFormularioAsync("/Platos/Delete/1", "/Platos/Delete/1", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Platos.CountAsync(p => p.CodPlato == 1)));
    }

    [Fact]
    public async Task Dado_un_plato_nuevo_valido_Cuando_se_crea_Entonces_aparece_en_el_menu()
    {
        var respuesta = await EnviarFormularioAsync("/Platos/Create", "/Platos/Create",
            new() { ["Nombre"] = "Chairo", ["Precio"] = "22.50" });

        Assert.True(respuesta.EsRedireccion);
        Assert.Contains("Chairo", await LeerAsync("/Platos"));
    }

    // ---------------------------------------------------------------- Reservas

    [Fact]
    public async Task Dada_una_mesa_de_4_Cuando_se_reservan_5_personas_Entonces_se_rechaza_por_capacidad()
    {
        var respuesta = await CrearReserva(cliente: 1, mesa: 2, personas: 5, FechaFutura());

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Contains("La mesa 2 tiene capacidad para 4 personas.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dada_una_reserva_existente_Cuando_otra_cae_dentro_de_2_horas_en_la_misma_mesa_Entonces_se_rechaza()
    {
        var primera = await CrearReserva(1, 2, 2, FechaFutura(hora: 20));
        Assert.True(primera.EsRedireccion);

        var segunda = await CrearReserva(2, 2, 2, FechaFutura(hora: 21));

        Assert.Equal(HttpStatusCode.OK, segunda.Estado);
        Assert.Contains("ya tiene una reserva en un horario cercano", segunda.Cuerpo);
    }

    [Fact]
    public async Task Dada_una_reserva_existente_Cuando_otra_se_hace_con_2_horas_o_mas_de_diferencia_Entonces_se_acepta()
    {
        await CrearReserva(1, 2, 2, FechaFutura(hora: 18));

        var segunda = await CrearReserva(2, 2, 2, FechaFutura(hora: 20));

        Assert.True(segunda.EsRedireccion);
    }

    [Fact]
    public async Task Dada_una_reserva_cancelada_Cuando_otra_ocupa_el_mismo_horario_Entonces_se_acepta()
    {
        await CrearReserva(1, 2, 2, FechaFutura(hora: 20), estado: "Cancelada");

        var segunda = await CrearReserva(2, 2, 2, FechaFutura(hora: 20));

        Assert.True(segunda.EsRedireccion);
    }

    [Fact]
    public async Task Dada_una_fecha_pasada_Cuando_se_registra_una_reserva_nueva_Entonces_se_rechaza()
    {
        var respuesta = await CrearReserva(1, 2, 2, "2026-01-10T20:00");

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Contains("La fecha de una reserva nueva no puede estar en el pasado.", respuesta.Cuerpo);
    }

    // ---------------------------------------------------------------- Pedidos

    [Fact]
    public async Task Dada_una_mesa_con_pedido_sin_facturar_Cuando_se_crea_otro_pedido_en_ella_Entonces_se_rechaza()
    {
        // La mesa 5 tiene el pedido 5 abierto en los datos de ejemplo.
        var respuesta = await EnviarFormularioAsync("/Pedidos/Create", "/Pedidos/Create", new()
        {
            ["FechaHora"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm"),
            ["Estado"] = "Abierto",
            ["NumMesa"] = "5",
            ["IdEmpleado"] = "1"
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Contains("La mesa 5 ya tiene un pedido sin facturar.", respuesta.Cuerpo);
    }

    [Fact]
    public async Task Dada_una_mesa_libre_Cuando_se_abre_un_pedido_rapido_Entonces_queda_abierto()
    {
        var respuesta = await EnviarFormularioAsync("/Platos/Create", "/Pedidos/AbrirRapido",
            new() { ["numMesa"] = "1", ["idEmpleado"] = "2" });

        Assert.True(respuesta.EsRedireccion);
        var pedido = await ConBaseDeDatosAsync(bd => bd.Pedidos.OrderByDescending(p => p.IdPedido).FirstAsync());
        Assert.Equal((EstadoPedido.Abierto, 1, 2), (pedido.Estado, pedido.NumMesa, pedido.IdEmpleado));
    }

    [Fact]
    public async Task Dada_una_mesa_ocupada_Cuando_se_abre_un_pedido_rapido_Entonces_no_se_crea_otro()
    {
        var antes = await ConBaseDeDatosAsync(bd => bd.Pedidos.CountAsync());

        await EnviarFormularioAsync("/Platos/Create", "/Pedidos/AbrirRapido", new() { ["numMesa"] = "5", ["idEmpleado"] = "1" });

        Assert.Equal(antes, await ConBaseDeDatosAsync(bd => bd.Pedidos.CountAsync()));
    }

    [Fact]
    public async Task Dado_un_pedido_abierto_Cuando_se_agrega_el_mismo_plato_dos_veces_Entonces_se_suma_la_cantidad()
    {
        // Pedido 5 ya tiene 6 salteñas (plato 4).
        await EnviarFormularioAsync("/Pedidos/Details/5", "/Pedidos/AgregarPlato",
            new() { ["NuevoPlato.IdPedido"] = "5", ["NuevoPlato.CodPlato"] = "4", ["NuevoPlato.Cantidad"] = "2" });

        var detalle = await ConBaseDeDatosAsync(bd => bd.PedidoPlatos.SingleAsync(d => d.IdPedido == 5 && d.CodPlato == 4));
        Assert.Equal(8, detalle.Cantidad);
    }

    [Fact]
    public async Task Dado_un_pedido_facturado_Cuando_se_intenta_agregar_un_plato_Entonces_no_se_modifica()
    {
        await EnviarFormularioAsync("/Platos/Create", "/Pedidos/AgregarPlato",
            new() { ["NuevoPlato.IdPedido"] = "1", ["NuevoPlato.CodPlato"] = "6", ["NuevoPlato.Cantidad"] = "1" });

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.PedidoPlatos.CountAsync(d => d.IdPedido == 1 && d.CodPlato == 6)));
    }

    [Fact]
    public async Task Dado_un_pedido_con_factura_Cuando_se_intenta_eliminar_Entonces_se_conserva()
    {
        await EnviarFormularioAsync("/Pedidos/Delete/1", "/Pedidos/Delete/1", new());

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Pedidos.CountAsync(p => p.IdPedido == 1)));
    }

    // ---------------------------------------------------------------- Facturas

    private Task<RespuestaWeb> Facturar(int idPedido, string nit, string fechaEmision) =>
        EnviarFormularioAsync("/Platos/Create", "/Facturas/Create", new()
        {
            ["IdPedido"] = idPedido.ToString(),
            ["NIT"] = nit,
            ["FechaEmision"] = fechaEmision
        });

    [Fact]
    public async Task Dado_un_pedido_abierto_con_platos_Cuando_se_factura_Entonces_pasa_a_Facturado()
    {
        var respuesta = await Facturar(5, "5566778899", "2026-10-26T13:00");

        Assert.True(respuesta.EsRedireccion);
        var pedido = await ConBaseDeDatosAsync(bd => bd.Pedidos.SingleAsync(p => p.IdPedido == 5));
        Assert.Equal(EstadoPedido.Facturado, pedido.Estado);
    }

    [Fact]
    public async Task Dado_un_NIT_invalido_Cuando_se_factura_Entonces_se_rechaza_y_el_pedido_sigue_abierto()
    {
        var respuesta = await Facturar(5, "ABC", "2026-10-26T13:00");

        Assert.Contains("El NIT debe contener solo dígitos (mínimo 5).", respuesta.Cuerpo);
        var pedido = await ConBaseDeDatosAsync(bd => bd.Pedidos.SingleAsync(p => p.IdPedido == 5));
        Assert.Equal(EstadoPedido.Abierto, pedido.Estado);
    }

    [Fact]
    public async Task Dado_un_pedido_ya_facturado_Cuando_se_factura_otra_vez_Entonces_no_se_duplica()
    {
        await Facturar(1, "1020304011", "2026-10-26T13:00");

        Assert.Equal(1, await ConBaseDeDatosAsync(bd => bd.Facturas.CountAsync(f => f.IdPedido == 1)));
    }

    [Fact]
    public async Task Dado_un_pedido_sin_platos_Cuando_se_intenta_facturar_Entonces_se_rechaza()
    {
        await ConBaseDeDatosAsync(async bd =>
        {
            bd.Pedidos.Add(new Pedido { FechaHora = DateTime.Now, Estado = EstadoPedido.Abierto, NumMesa = 1, IdEmpleado = 1 });
            return await bd.SaveChangesAsync();
        });
        var idVacio = await ConBaseDeDatosAsync(bd => bd.Pedidos.MaxAsync(p => p.IdPedido));

        await Facturar(idVacio, "5566778899", DateTime.Now.ToString("yyyy-MM-ddTHH:mm"));

        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Facturas.CountAsync(f => f.IdPedido == idVacio)));
    }

    [Fact]
    public async Task Dada_una_factura_Cuando_se_anula_Entonces_el_pedido_vuelve_a_Servido()
    {
        await EnviarFormularioAsync("/Facturas/Delete/1", "/Facturas/Delete/1", new());

        var pedido = await ConBaseDeDatosAsync(bd => bd.Pedidos.SingleAsync(p => p.IdPedido == 1));
        Assert.Equal(EstadoPedido.Servido, pedido.Estado);
        Assert.Equal(0, await ConBaseDeDatosAsync(bd => bd.Facturas.CountAsync(f => f.NumFact == 1)));
    }

    [Fact]
    public async Task Dada_una_fecha_de_emision_anterior_al_pedido_Cuando_se_factura_Entonces_se_rechaza()
    {
        // El pedido 5 se abrió el 25/10/2026 12:45.
        var respuesta = await Facturar(5, "5566778899", "2026-10-20T10:00");

        Assert.Equal(HttpStatusCode.OK, respuesta.Estado);
        Assert.Contains("La fecha de emisión no puede ser anterior a la del pedido.", respuesta.Cuerpo);
    }
}
