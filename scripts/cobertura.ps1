<#  Ejecuta unitarias+integración con cobertura y genera Docs/evidencias/cobertura.md  #>
$raiz = Split-Path -Parent $PSScriptRoot
Remove-Item "$raiz/PedidoHub.Tests/TestResults" -Recurse -Force -ErrorAction SilentlyContinue
dotnet test "$raiz/PedidoHub.Tests" --nologo --collect:"XPlat Code Coverage" --logger "trx;LogFileName=unit-integracion.trx" --results-directory "$raiz/PedidoHub.Tests/TestResults" | Select-String 'Correctas|Con error' | ForEach-Object Line
New-Item -ItemType Directory -Force "$raiz/Docs/evidencias/resultados" | Out-Null
Copy-Item (Get-ChildItem "$raiz/PedidoHub.Tests/TestResults" -Recurse -Filter unit-integracion.trx | Select-Object -First 1).FullName "$raiz/Docs/evidencias/resultados/" -Force
[xml]$c = Get-Content (Get-ChildItem "$raiz/PedidoHub.Tests/TestResults" -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1).FullName
$filas = $c.coverage.packages.package.classes.class | Where-Object { $_.filename -match '^(Controllers|Services|Models|ViewModels|Data\\PedidoHubDbContext)' } | Group-Object filename | ForEach-Object {
  $l = @($_.Group | ForEach-Object { $_.lines.line } | Group-Object number | ForEach-Object { $_.Group | Sort-Object { [int]$_.hits } -Descending | Select-Object -First 1 })
  $h = @($l | Where-Object { [int]$_.hits -gt 0 }).Count
  [pscustomobject]@{ Archivo = $_.Name; Cubiertas = $h; Validas = $l.Count; Pct = [math]::Round(100 * $h / [math]::Max($l.Count, 1), 1) } }
$tc = ($filas | Measure-Object Cubiertas -Sum).Sum; $tv = ($filas | Measure-Object Validas -Sum).Sum
$md = "# Cobertura de código (coverlet)`n`nGenerado el $(Get-Date -Format 'yyyy-MM-dd HH:mm'). Alcance: Controllers, Services, Models, ViewModels y DbContext (se excluyen migraciones y vistas Razor generadas).`n`n**Cobertura de líneas: $tc / $tv = $([math]::Round(100*$tc/$tv,1)) %**`n`n| Archivo | Líneas cubiertas | Líneas válidas | % |`n|---|---|---|---|`n"
$filas | ForEach-Object { $md += "| $($_.Archivo.Replace('\','/')) | $($_.Cubiertas) | $($_.Validas) | $($_.Pct) % |`n" }
Set-Content "$raiz/Docs/evidencias/cobertura.md" $md -Encoding UTF8
"Cobertura: $tc/$tv = $([math]::Round(100*$tc/$tv,1))%"
