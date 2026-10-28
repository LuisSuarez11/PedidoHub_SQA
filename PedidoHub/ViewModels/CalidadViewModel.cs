using System.Text.Json.Serialization;

namespace PedidoHub.ViewModels;

public class SuiteDePruebas
{
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    [JsonPropertyName("herramienta")] public string Herramienta { get; set; } = "";
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("pasadas")] public int Pasadas { get; set; }
}

public class ArchivoCobertura
{
    [JsonPropertyName("archivo")] public string Archivo { get; set; } = "";
    [JsonPropertyName("cubiertas")] public int Cubiertas { get; set; }
    [JsonPropertyName("validas")] public int Validas { get; set; }
    public double Porcentaje => Validas == 0 ? 0 : Math.Round(100.0 * Cubiertas / Validas, 1);
}

public class DefectoRegistrado
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
    [JsonPropertyName("severidad")] public string Severidad { get; set; } = "";
    [JsonPropertyName("estado")] public string Estado { get; set; } = "";
    [JsonPropertyName("prueba")] public string Prueba { get; set; } = "";
}

public class HistoriaTrazada
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
    [JsonPropertyName("prioridad")] public string Prioridad { get; set; } = "";
    [JsonPropertyName("pruebas")] public string Pruebas { get; set; } = "";
    [JsonPropertyName("estado")] public string Estado { get; set; } = "";
}

public class CriterioDod
{
    [JsonPropertyName("texto")] public string Texto { get; set; } = "";
    [JsonPropertyName("cumple")] public bool Cumple { get; set; }
}

public class DatosCalidad
{
    [JsonPropertyName("actualizado")] public string Actualizado { get; set; } = "";
    [JsonPropertyName("sprint")] public string Sprint { get; set; } = "";
    [JsonPropertyName("pruebas")] public List<SuiteDePruebas> Pruebas { get; set; } = new();
    [JsonPropertyName("cobertura")] public List<ArchivoCobertura> Cobertura { get; set; } = new();
    [JsonPropertyName("defectos")] public List<DefectoRegistrado> Defectos { get; set; } = new();
    [JsonPropertyName("historias")] public List<HistoriaTrazada> Historias { get; set; } = new();
    [JsonPropertyName("dod")] public List<CriterioDod> Dod { get; set; } = new();
    [JsonPropertyName("metaCobertura")] public int MetaCobertura { get; set; } = 80;
}

public class CalidadViewModel
{
    public DatosCalidad Datos { get; set; } = new();
    public bool BaseDeDatosDisponible { get; set; }
    public Dictionary<string, int> Registros { get; set; } = new();

    public int TotalPruebas => Datos.Pruebas.Sum(p => p.Total);
    public int TotalPasadas => Datos.Pruebas.Sum(p => p.Pasadas);
    public double TasaExito => TotalPruebas == 0 ? 0 : Math.Round(100.0 * TotalPasadas / TotalPruebas, 1);
    public int LineasCubiertas => Datos.Cobertura.Sum(c => c.Cubiertas);
    public int LineasValidas => Datos.Cobertura.Sum(c => c.Validas);
    public double CoberturaGlobal => LineasValidas == 0 ? 0 : Math.Round(100.0 * LineasCubiertas / LineasValidas, 1);
    public int DefectosCerrados => Datos.Defectos.Count(d => d.Estado == "Cerrado");
    public double DensidadDefectos => LineasValidas == 0 ? 0 : Math.Round(Datos.Defectos.Count / (LineasValidas / 1000.0), 1);
}