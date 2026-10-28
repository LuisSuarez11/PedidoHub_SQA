# Tablero Kanban — PedidoHub (integración del Test Plan con el tablero ágil)

El tablero se mantiene en **Trello** (equivalente en Jira: proyecto Kanban); se crea importando el CSV. El archivo [`backlog-trello.csv`](backlog-trello.csv) contiene todas las tarjetas listas para importar (Trello: *Power-Up CSV Importer*; Jira: *Importar incidencias desde CSV*).

## Columnas y política (WIP)
| Columna | Política de entrada | WIP |
|---|---|---|
| Backlog | Historia o defecto registrado | — |
| En análisis | Cumple **Definition of Ready** (criterios Gherkin, prioridad, datos de prueba) | 3 |
| En desarrollo | Pruebas escritas primero (rojo) | 3 |
| En pruebas | Implementada; CI en ejecución; revisión QA | 3 |
| Hecho | Cumple **Definition of Done** | — |

Etiquetas: `P1` crítica, `P2` alta, `P3` media (prioridad por valor, sección 3.4 del plan), `Defecto`, `Deuda técnica`.

## Estado al cierre del sprint (28/10/2026)
| Backlog | En análisis | En desarrollo | En pruebas | Hecho |
|---|---|---|---|---|
| TD-01 Claves y bloqueo de intentos | — | — | — | HU-01 … HU-17 (17 historias) |
| TD-02 Paginación | | | | DEF-01 Reserva en fecha pasada |
| TD-03 Fuentes locales | | | | DEF-02 Dos pedidos por mesa |
| TD-04 Cobertura Telefonos | | | | DEF-03 Factura con fecha anterior |
| TD-05 Análisis estático | | | | DEF-04 Endpoint /health |
| | | | | DEF-05 Cabeceras de seguridad |
| | | | | DEF-06 Sin autenticación ni roles |

## Cómo se integra el plan de pruebas con el tablero
1. **Cada tarjeta de historia** lleva sus criterios de aceptación y la lista de pruebas que la verifican (columna *Description* del CSV).
2. **Checklist en cada tarjeta:** DoR (al entrar) y DoD (al salir). Una tarjeta no pasa a *Hecho* con el CI en rojo.
3. **Los defectos** son tarjetas con severidad; al cerrarse enlazan la prueba de regresión.
4. **Métricas del sprint** (throughput, lead time, pass rate) se toman del tablero y del CI al cierre.

## Métricas de flujo del sprint
| Métrica | Valor |
|---|---|
| Tarjetas terminadas (throughput) | 23 (17 historias + 6 defectos) |
| Tarjetas nuevas de deuda técnica | 5 (TD-01 a TD-05) |
| WIP máximo respetado | 3 |