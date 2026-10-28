using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PedidoHub.Data;
using PedidoHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PedidoHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PedidoHub")));
builder.Services.AddScoped<ReporteService>();
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
app.UseAuthorization();

// Endpoint de salud para CI/CD y monitoreo: responde "Healthy" si la aplicación alcanza la base de datos.
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Expone Program a los proyectos de pruebas (WebApplicationFactory).
public partial class Program { }
