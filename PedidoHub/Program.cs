using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Models;
using PedidoHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PedidoHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PedidoHub")));
builder.Services.AddScoped<ReporteService>();
builder.Services.AddSingleton<CalidadService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Ingresar";
        opciones.AccessDeniedPath = "/Cuenta/Denegado";
        opciones.Cookie.Name = "PedidoHub.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;
    });
// Todo requiere sesión salvo lo marcado con [AllowAnonymous] (landing, login, /health).
builder.Services.AddAuthorization(opciones =>
    opciones.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddHealthChecks().AddDbContextCheck<PedidoHubDbContext>("base-de-datos");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Cultura fija para que decimales y fechas se interpreten igual en cualquier equipo.
var cultura = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new[] { cultura },
    SupportedUICultures = new[] { cultura }
});

// Cabeceras de seguridad básicas (OWASP): sin sniffing de tipos, sin iframes externos y referrer mínimo.
app.Use(async (contexto, siguiente) =>
{
    var cabeceras = contexto.Response.Headers;
    cabeceras["X-Content-Type-Options"] = "nosniff";
    cabeceras["X-Frame-Options"] = "DENY";
    cabeceras["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await siguiente();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de salud para CI/CD y monitoreo: responde "Healthy" si la aplicación alcanza la base de datos.
app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await InicializadorUsuarios.InicializarAsync(app.Services, app.Configuration);

app.Run();

// Expone Program a los proyectos de pruebas (WebApplicationFactory).
public partial class Program { }
