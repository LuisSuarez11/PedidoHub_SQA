# PedidoHub

Aplicación web en **ASP.NET Core MVC (.NET 8)** con **Entity Framework Core** y **SQL Server** para gestionar un restaurante: clientes y sus teléfonos, reservas de mesas, pedidos con sus platos, facturación y reportes de ventas.

## Autor

| Nombre | Usuario de GitHub |
|---|---|
| Luis Andrés Suárez Andrade | [LuisSuarez11](https://github.com/LuisSuarez11) |

## Requisitos

- .NET SDK 8.0 o superior
- SQL Server LocalDB (incluido con Visual Studio) o cualquier instancia de SQL Server
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

## Cómo ejecutar

1. Revisar la cadena de conexión `PedidoHub` en `PedidoHub/appsettings.json` (por defecto usa LocalDB con autenticación de Windows, sin contraseñas).
2. Crear la base de datos con sus datos de ejemplo mediante migraciones:

```bash
cd PedidoHub
dotnet ef database update
```

3. Ejecutar la aplicación:

```bash
dotnet run
```

4. Abrir la dirección que indique la consola (por ejemplo `http://localhost:5158`).

La migración `CrearModeloPedidoHub` crea las tablas y carga datos de ejemplo (clientes, mesas, empleados, platos, reservas, pedidos y facturas).

## Funcionalidades

- **CRUD** de Clientes (con sus teléfonos), Mesas, Empleados, Platos, Reservas, Pedidos (con sus platos) y Facturas.
- **Validaciones**: campos obligatorios y longitudes, cliente con al menos un teléfono (y sin poder borrar el último), rango de capacidad y precios, formato de teléfono y NIT, nombre de plato único, personas de la reserva no mayores a la capacidad de la mesa, mesa sin reservas superpuestas (2 horas), una sola factura por pedido, no eliminar registros con datos relacionados.
- **Plano de mesas** en la portada ("Servicio"): cada mesa se dibuja con sus sillas y muestra si está libre, ocupada o reservada. Al tocarla se abre un panel lateral con su pedido, y una mesa libre permite abrir un pedido en un paso.
- **Paleta de búsqueda** (`Ctrl + K`): encuentra clientes, platos, empleados, mesas, pedidos y facturas, y salta a cualquier pantalla.
- **Listas con pestañas por estado y filtro instantáneo** en Pedidos y Reservas; avisos de confirmación y de error como notificaciones.
- **Total del pedido** calculado (`Σ Cantidad × Precio`), no almacenado, como atributo derivado del DER.

## Reportes

1. **Ventas por período** (`Reportes > Ventas por período`): lista las facturas emitidas con filtros por rango de fechas y empleado. Muestra cantidad de facturas, total facturado y ticket promedio. Sirve para conocer cuánto vendió el restaurante y el desempeño de cada empleado.
2. **Platos más vendidos** (`Reportes > Platos más vendidos`): unidades vendidas y monto generado por plato (solo pedidos facturados, según la fecha de emisión de la factura), con filtro de fechas y cantidad de platos a mostrar. Sirve para decidir qué platos promocionar o retirar del menú.

## Estructura del proyecto

```
PedidoHub/
├── Controllers/     Un controlador por entidad + Reportes y Home
├── Models/          Entidades y enumeraciones (EstadoReserva, EstadoPedido)
├── ViewModels/      Modelos de formularios, detalle y reportes
├── Services/        ReporteService (consultas de los reportes)
├── Data/            DbContext, configuraciones Fluent API (con datos iniciales) y migraciones
└── Views/           Una carpeta por controlador + Shared
Docs/                DER y explicación de cambios
```

## DER

Ver la carpeta `Docs/` y `Docs/CAMBIOS_DER.md`.

## Aseguramiento de la calidad (SQA)

Este repositorio es la versión de la asignatura **SQA**. Incluye el Test Plan ágil, 216 pruebas automatizadas y la auditoría interna.

| Qué | Dónde |
|---|---|
| Test Plan ágil | `Docs/TestPlan_Agil.docx` / `.pdf` / `.md` |
| Informe de auditoría interna | `Docs/InformeAuditoria.md` |
| Tablero Kanban y backlog importable | `Docs/TableroKanban.md`, `Docs/backlog-trello.csv` |
| Evidencias (TRX, JUnit, cobertura, fase roja TDD) | `Docs/evidencias/` |
| Unitarias + integración (xUnit, 180) | `PedidoHub.Tests/` |
| E2E Selenium (17) | `PedidoHub.E2E/` |
| E2E Cypress (19) | `cypress/` |
| CI/CD | `.github/workflows/ci.yml` |

```bash
dotnet test PedidoHub.Tests                      # unitarias + integración (SQLite en memoria)
powershell scripts/cobertura.ps1                 # + cobertura en Docs/evidencias/cobertura.md
powershell scripts/run-e2e.ps1                   # Selenium + Cypress contra LocalDB (PedidoHubDB_QA)
```

Mejoras añadidas a la aplicación durante el trabajo de SQA:

- **Landing pública** en `/` con funciones, roles e indicadores de calidad.
- **Inicio de sesión y roles** (Administrador, Cajero, Mesero): cookie HttpOnly, claves con hash PBKDF2, acceso denegado por rol.
- **Bitácora de auditoría** (`/Bitacora`): quién creó, modificó o eliminó cada dato, con valor anterior y nuevo.
- **Panel de calidad** (`/Calidad`): métricas del Test Plan junto al estado vivo del sistema.
- **Exportación a CSV** de los reportes de ventas y de platos.
- Validación de reservas en el pasado, una sola cuenta abierta por mesa, fecha de factura coherente con el pedido, endpoint `/health` y cabeceras de seguridad.

### Usuarios de demostración

| Usuario | Rol |
|---|---|
| `admin` | Administrador |
| `cajero` | Cajero |
| `mesero` | Mesero |

La contraseña inicial de los tres es `PedidoHub2026!` (configurable con `Seed:Clave`). Al iniciar, la aplicación aplica las migraciones en SQL Server y crea estos usuarios si no existen. **Cámbiela en cualquier despliegue real.**