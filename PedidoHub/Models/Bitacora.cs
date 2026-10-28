namespace PedidoHub.Models;

/// <summary>Registro de auditoría: quién cambió qué y cuándo (trazabilidad).</summary>
public class Bitacora
{
    public int IdBitacora { get; set; }
    public DateTime FechaHora { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Registro { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
}