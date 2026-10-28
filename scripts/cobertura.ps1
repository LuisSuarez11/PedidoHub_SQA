<#
  Ejecuta unitarias + integración con cobertura y actualiza:
    - Docs/evidencias/cobertura.md
    - PedidoHub/Data/calidad.json  (alimenta el Panel de calidad y la landing)
  Los resultados E2E se toman de Docs/evidencias/resultados (los deja scripts/run-e2e.ps1).
#>
$raiz = Split-Path -Parent $PSScriptRoot
Remove-Item "$raiz/PedidoHub.Tests/TestResults" -Recurse -Force -ErrorAction SilentlyContinue
dotnet test "$raiz/PedidoHub.Tests" --nologo --collect:"XPlat Code Coverage" --logger "trx;LogFileName=unit-integracion.trx" --results-directory "$raiz/PedidoHub.Tests/TestResults" | Select-String 'Correctas|Con error|Passed|Failed' | ForEach-Object Line
$res = "$raiz/Docs/evidencias/resultados"
New-Item -ItemType Directory -Force $res | Out-Null
$trx = (Get-ChildItem "$raiz/PedidoHub.Tests/TestResults" -Recurse -Filter unit-integracion.trx | Select-Object -First 1).FullName
Copy-Item $trx "$res/" -Force

# --- Suites xUnit (separadas por namespace)
[xml]$t = Get-Content $trx
$clase = @{}; $t.TestRun.TestDefinitions.UnitTest | ForEach-Object { $clase[$_.id] = $_.TestMethod.className }
$resultados = $t.TestRun.Results.UnitTestResult | ForEach-Object { [pscustomobject]@{ Clase = $clase[$_.testId]; Ok = ($_.outcome -eq 'Passed') } }
function Suite($nombre, $herramienta, $total, $pasadas) { [ordered]@{ nombre = $nombre; herramienta = $herramienta; total = $total; pasadas = $pasadas } }
$unit = $resultados | Where-Object { $_.Clase -like '*.Unit.*' }
$integ = $resultados | Where-Object { $_.Clase -like '*.Integration.*' }
$suites = @(
  (Suite 'Unitarias' 'xUnit' @($unit).Count @($unit | Where-Object Ok).Count),
  (Suite 'Integración y aceptación (BDD)' 'xUnit + WebApplicationFactory' @($integ).Count @($integ | Where-Object Ok).Count)
)
$sel = "$res/selenium.trx"
if (Test-Path $sel) { [xml]$s = Get-Content $sel; $c = $s.TestRun.ResultSummary.Counters; $suites += ,(Suite 'E2E navegador' 'Selenium WebDriver' ([int]$c.total) ([int]$c.passed)) }
$cy = Get-ChildItem $res -Filter 'cypress-*.xml' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($cy) { [xml]$x = Get-Content $cy.FullName; $tot = [int]$x.testsuites.tests; $fal = [int]$x.testsuites.failures; $suites += ,(Suite 'E2E navegador' 'Cypress' $tot ($tot - $fal)) }

# --- Cobertura (lógica de la aplicación; sin migraciones ni vistas generadas)
[xml]$c = Get-Content (Get-ChildItem "$raiz/PedidoHub.Tests/TestResults" -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1).FullName
$filas = $c.coverage.packages.package.classes.class | Where-Object { $_.filename -match '^(Controllers|Services|Models|ViewModels|Data\\PedidoHubDbContext)' } | Group-Object filename | ForEach-Object {
  $l = @($_.Group | ForEach-Object { $_.lines.line } | Group-Object number | ForEach-Object { $_.Group | Sort-Object { [int]$_.hits } -Descending | Select-Object -First 1 })
  $h = @($l | Where-Object { [int]$_.hits -gt 0 }).Count
  [pscustomobject]@{ Archivo = $_.Name.Replace('\', '/'); Cubiertas = $h; Validas = $l.Count; Pct = [math]::Round(100 * $h / [math]::Max($l.Count, 1), 1) } }
$tc = ($filas | Measure-Object Cubiertas -Sum).Sum; $tv = ($filas | Measure-Object Validas -Sum).Sum
$md = "# Cobertura de código (coverlet)`n`nGenerado el $(Get-Date -Format 'yyyy-MM-dd HH:mm'). Alcance: Controllers, Services, Models, ViewModels y DbContext (se excluyen migraciones y vistas Razor generadas).`n`n**Cobertura de líneas: $tc / $tv = $([math]::Round(100*$tc/$tv,1)) %**`n`n| Archivo | Líneas cubiertas | Líneas válidas | % |`n|---|---|---|---|`n"
$filas | ForEach-Object { $md += "| $($_.Archivo) | $($_.Cubiertas) | $($_.Validas) | $($_.Pct) % |`n" }
$sinBom = New-Object Text.UTF8Encoding $false
[IO.File]::WriteAllText("$raiz/Docs/evidencias/cobertura.md", $md, $sinBom)

# --- calidad.json
$ruta = "$raiz/PedidoHub/Data/calidad.json"
$j = [IO.File]::ReadAllText($ruta, [Text.Encoding]::UTF8) | ConvertFrom-Json
$j.actualizado = Get-Date -Format 'yyyy-MM-dd'
$j.pruebas = $suites
$j.cobertura = @($filas | ForEach-Object { [ordered]@{ archivo = $_.Archivo; cubiertas = $_.Cubiertas; validas = $_.Validas } })
[IO.File]::WriteAllText($ruta, ($j | ConvertTo-Json -Depth 6), $sinBom)
"Cobertura: $tc/$tv = $([math]::Round(100*$tc/$tv,1))%  |  Suites: " + (($suites | ForEach-Object { "$($_.herramienta) $($_.pasadas)/$($_.total)" }) -join '; ')