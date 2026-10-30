<#
  Ejecuta TODAS las pruebas y deja la evidencia en Docs/evidencias/defensa:
    - consola de xUnit (cada prueba con su resultado), TRX, cobertura
    - Selenium: TRX + una captura de pantalla por prueba
    - Cypress: JUnit + video + una captura por escenario
    - RESUMEN.md con fecha, equipo y conteos
  Uso:  powershell -ExecutionPolicy Bypass -File scripts/evidencia-defensa.ps1
#>
$ErrorActionPreference = 'Continue'
$raiz = Split-Path -Parent $PSScriptRoot
$dest = "$raiz/Docs/evidencias/defensa"
Remove-Item $dest -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force "$dest/capturas_selenium", "$dest/capturas_cypress", "$dest/video_cypress" | Out-Null
Remove-Item "$raiz/Docs/evidencias/resultados" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$raiz/cypress/screenshots", "$raiz/cypress/videos" -Recurse -Force -ErrorAction SilentlyContinue
$inicio = Get-Date

Write-Host '==> 1/3 E2E (Selenium + Cypress) con capturas y video...'
$env:E2E_CAPTURAS = "$dest/capturas_selenium"
powershell -ExecutionPolicy Bypass -File "$PSScriptRoot/run-e2e.ps1" 2>&1 | Out-String -Width 220 | Set-Content "$dest/02_e2e_consola.txt" -Encoding UTF8
Remove-Item Env:E2E_CAPTURAS

Write-Host '==> 2/3 Unitarias + integración + cobertura...'
powershell -ExecutionPolicy Bypass -File "$PSScriptRoot/cobertura.ps1" 2>&1 | Out-String -Width 220 | Set-Content "$dest/01_cobertura_consola.txt" -Encoding UTF8
dotnet test "$raiz/PedidoHub.Tests" --no-build --logger "console;verbosity=normal" *>&1 | Out-String -Width 220 | Set-Content "$dest/01_xunit_detalle_cada_prueba.txt" -Encoding UTF8

Write-Host '==> 3/3 Reuniendo evidencia...'
Copy-Item "$raiz/Docs/evidencias/resultados/*" $dest -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item "$raiz/Docs/evidencias/cobertura.md" $dest -Force
Get-ChildItem "$raiz/cypress/screenshots" -Recurse -Filter *.png -ErrorAction SilentlyContinue | Copy-Item -Destination "$dest/capturas_cypress" -Force
Get-ChildItem "$raiz/cypress/videos" -Recurse -Filter *.mp4 -ErrorAction SilentlyContinue | Copy-Item -Destination "$dest/video_cypress" -Force

$calidad = Get-Content "$raiz/PedidoHub/Data/calidad.json" -Raw -Encoding UTF8 | ConvertFrom-Json
$filas = ($calidad.pruebas | ForEach-Object { "| $($_.nombre) | $($_.herramienta) | $($_.pasadas) / $($_.total) |" }) -join "`n"
$tot = ($calidad.pruebas | Measure-Object total -Sum).Sum; $pas = ($calidad.pruebas | Measure-Object pasadas -Sum).Sum
$cub = ($calidad.cobertura | Measure-Object cubiertas -Sum).Sum; $val = ($calidad.cobertura | Measure-Object validas -Sum).Sum
$resumen = @"
# Evidencia de ejecución de pruebas

- **Fecha y hora:** $(Get-Date -Format 'dd/MM/yyyy HH:mm') (duración total: $([int]((Get-Date) - $inicio).TotalMinutes) min)
- **Equipo:** $env:COMPUTERNAME · $((Get-CimInstance Win32_OperatingSystem).Caption)
- **Versiones:** .NET SDK $(dotnet --version) · Node $(node -v) · Cypress $((Get-Content "$raiz/cypress/package.json" -Raw | ConvertFrom-Json).devDependencies.cypress)
- **Comando:** ``scripts/evidencia-defensa.ps1``

| Suite | Herramienta | Pasadas / Total |
|---|---|---|
$filas
| **Total** | | **$pas / $tot** |

Cobertura de líneas de la lógica: **$cub / $val = $([math]::Round(100*$cub/$val,1)) %**

## Qué hay en esta carpeta
| Archivo o carpeta | Contenido |
|---|---|
| ``01_xunit_detalle_cada_prueba.txt`` | Cada prueba xUnit con su resultado y duración |
| ``01_cobertura_consola.txt`` / ``cobertura.md`` | Ejecución con cobertura y detalle por archivo |
| ``unit-integracion.trx`` | Reporte oficial de xUnit (unitarias e integración) |
| ``selenium.trx`` y ``capturas_selenium/`` | Reporte de Selenium y una captura por prueba |
| ``cypress-*.xml``, ``capturas_cypress/`` y ``video_cypress/`` | Reporte JUnit, captura por escenario y video de la ejecución |
| ``02_e2e_consola.txt`` | Salida completa de Selenium y Cypress |
"@
Set-Content "$dest/RESUMEN.md" $resumen -Encoding UTF8
Write-Host "Evidencia lista en $dest"