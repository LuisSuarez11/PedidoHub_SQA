# Evidencia de ejecución de pruebas

- **Fecha y hora:** 30/10/2026 01:47 (duración total: 2 min)
- **Equipo:** ZEWPHY · Microsoft Windows 11 Home
- **Versiones:** .NET SDK 10.0.401 · Node v24.15.0 · Cypress 13.15.0
- **Comando:** `scripts/evidencia-defensa.ps1`

| Suite | Herramienta | Pasadas / Total |
|---|---|---|
| Unitarias | xUnit | 46 / 46 |
| Integración y aceptación (BDD) | xUnit + WebApplicationFactory | 134 / 134 |
| E2E navegador | Selenium WebDriver | 17 / 17 |
| E2E navegador | Cypress | 19 / 19 |
| **Total** | | **216 / 216** |

Cobertura de líneas de la lógica: **1332 / 1633 = 81.6 %**

## Qué hay en esta carpeta
| Archivo o carpeta | Contenido |
|---|---|
| `01_xunit_detalle_cada_prueba.txt` | Cada prueba xUnit con su resultado y duración |
| `01_cobertura_consola.txt` / `cobertura.md` | Ejecución con cobertura y detalle por archivo |
| `unit-integracion.trx` | Reporte oficial de xUnit (unitarias e integración) |
| `selenium.trx` y `capturas_selenium/` | Reporte de Selenium y una captura por prueba |
| `cypress-*.xml`, `capturas_cypress/` y `video_cypress/` | Reporte JUnit, captura por escenario y video de la ejecución |
| `02_e2e_consola.txt` | Salida completa de Selenium y Cypress |
