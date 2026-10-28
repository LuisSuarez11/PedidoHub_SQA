using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;

namespace PedidoHub.Services;

/// <summary>Aplica las migraciones en SQL Server y crea los usuarios iniciales si no existen.</summary>
public static class InicializadorUsuarios
{
    public const string ClavePorDefecto = "PedidoHub2026!";

    public static async Task InicializarAsync(IServiceProvider servicios, IConfiguration configuracion)
    {
        using var alcance = servicios.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<PedidoHubDbContext>();
        if (contexto.Database.IsSqlServer())
        {
            await contexto.Database.MigrateAsync();
        }
        if (await contexto.Usuarios.AnyAsync())
        {
            return;
        }

        var clave = configuracion["Seed:Clave"] ?? ClavePorDefecto;
        var hasher = new PasswordHasher<Usuario>();
        foreach (var (usuario, nombre, rol) in new[]
        {
            ("admin", "Luis Andrés Suárez Andrade", Roles.Administrador),
            ("mesero", "Laura Gómez", Roles.Mesero),
            ("cajero", "Marta Rojas", Roles.Cajero)
        })
        {
            var nuevo = new Usuario { NombreUsuario = usuario, NombreCompleto = nombre, Rol = rol };
            nuevo.ClaveHash = hasher.HashPassword(nuevo, clave);
            contexto.Usuarios.Add(nuevo);
        }
        await contexto.SaveChangesAsync();
    }
}