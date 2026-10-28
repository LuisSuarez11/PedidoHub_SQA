<#
.SYNOPSIS
  Levanta PedidoHub contra una base de datos de pruebas (LocalDB, "PedidoHubDB_QA") y ejecuta las pruebas E2E.
.EXAMPLE
  ./scripts/run-e2e.ps1            # Selenium (xUnit) + Cypress
  ./scripts/run-e2e.ps1 -Solo selenium
#>
param([ValidateSet('todo','selenium','cypress')][string]$Solo = 'todo')

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$puerto = 5199
$url = "http://localhost:$puerto"
$env:ConnectionStrings__PedidoHub = "Server=(localdb)\mssqllocaldb;Database=PedidoHubDB_QA;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:PEDIDOHUB_URL = $url

Write-Host '==> Creando base de datos limpia PedidoHubDB_QA...'
Push-Location "$raiz/PedidoHub"
dotnet ef database drop --force | Out-Null
dotnet ef database update | Out-Null
Pop-Location

Write-Host "==> Iniciando la aplicación en $url ..."
$app = Start-Process dotnet -ArgumentList "run --no-launch-profile --project `"$raiz/PedidoHub`" --urls $url" -PassThru -WindowStyle Hidden
try {
    for ($i = 0; $i -lt 60; $i++) {
        try { if ((Invoke-WebRequest "$url/health" -UseBasicParsing -TimeoutSec 2).Content -eq 'Healthy') { break } } catch { Start-Sleep -Seconds 1 }
    }

    if ($Solo -in 'todo','selenium') {
        Write-Host '==> Selenium...'
        dotnet test "$raiz/PedidoHub.E2E" --logger "trx;LogFileName=selenium.trx" --results-directory "$raiz/Docs/evidencias/resultados"
    }
    if ($Solo -in 'todo','cypress') {
        Write-Host '==> Cypress...'
        Push-Location "$raiz/cypress"
        $env:CYPRESS_baseUrl = $url
        npx cypress run
        Pop-Location
    }
}
finally {
    Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match "--urls $url" } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
    if ($app -and -not $app.HasExited) { Stop-Process -Id $app.Id -Force }
}
