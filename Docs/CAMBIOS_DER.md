# Cambios entre el DER original y el DER actualizado

Archivos de esta carpeta:

| Archivo | Contenido |
|---|---|
| `DER_Original.png` | DER con el que se inició el proyecto |
| `DER_Actualizado.png` / `.pdf` | DER final, coherente con la aplicación implementada |
| `DER_Actualizado.drawio` | Mismo DER editable en [draw.io](https://app.diagrams.net) |

## Cambios realizados durante la implementación

| Elemento | DER original | DER actualizado | Motivo |
|---|---|---|---|
| Teléfono del cliente | Atributo multivaluado de CLIENTE | Entidad `TELEFONO_CLIENTE` (`ID_Telefono`, `Numero`) con relación 1:N desde CLIENTE | Un cliente puede tener varios teléfonos y el modelo relacional no admite atributos multivaluados |
| RESERVA | `ID_Reserva`, `Fecha_Hora` | Se agregan `Estado` y `Cant_Personas` | Seguir el ciclo de la reserva y validar la capacidad de la mesa |
| PEDIDO | `ID_Pedido`, `Estado`, `Total_Calc` | Se agrega `Fecha_Hora` | Filtrar por fechas y construir el reporte de ventas |
| EMPLEADO | `ID_Emp`, `Nombre` | Se agrega `Cargo` | Distinguir meseros y cajeros |
| FACTURA | `Num_Fact`, `NIT` | Se agrega `Fecha_Emision` | Es la fecha de venta del reporte por período |
| Notación de cardinalidades | (min,máx) escritos hacia el lado opuesto | (min,máx) de **participación**: cuántas veces interviene una ocurrencia de la entidad en la relación | Lectura estándar y sin ambigüedad |
| PEDIDO en `genera` | (1,1) | **(0,1)** | Un pedido abierto todavía no tiene factura |
| PEDIDO en `contiene` | (1,n) | **(0,n)** | Un pedido recién abierto aún no tiene platos; para facturar se exige al menos uno |

Las relaciones y las razones (1:N, 1:1, N:M) no cambiaron: Cliente 1:N Reserva, Mesa 1:N Reserva, Mesa 1:N Pedido, Empleado 1:N Pedido, Pedido 1:1 Factura y Pedido N:M Plato (con `Cantidad`).

## Cómo cumple la aplicación cada cardinalidad del DER final

| Relación | Cardinalidades | Dónde se cumple en el código |
|---|---|---|
| `posee` (Cliente – Teléfono) | TELEFONO (1,1), CLIENTE (1,n) | `TelefonoCliente.IdCliente` es obligatoria. Crear un cliente exige un teléfono (`ClienteFormViewModel`) y no se puede eliminar el último (`TelefonosController`) |
| `realiza` (Cliente – Reserva) | CLIENTE (0,n), RESERVA (1,1) | `Reserva.IdCliente` obligatoria; no se elimina un cliente con reservas |
| `tiene` (Mesa – Reserva) | MESA (0,n), RESERVA (1,1) | `Reserva.NumMesa` obligatoria; además se valida la capacidad y que no haya dos reservas activas con menos de 2 horas de diferencia |
| `registra` (Mesa – Pedido) | MESA (0,n), PEDIDO (1,1) | `Pedido.NumMesa` obligatoria; no se elimina una mesa con pedidos |
| `atiende` (Empleado – Pedido) | EMPLEADO (0,n), PEDIDO (1,1) | `Pedido.IdEmpleado` obligatoria; no se elimina un empleado con pedidos |
| `genera` (Pedido – Factura) | PEDIDO (0,1), FACTURA (1,1) | `Factura.IdPedido` obligatoria con índice único: un pedido tiene como máximo una factura |
| `contiene` (Pedido – Plato) | PEDIDO (0,n), PLATO (0,n) | Tabla `PedidoPlatos` con clave compuesta (`IdPedido`, `CodPlato`) y `Cantidad` mayor que 0; facturar exige al menos un plato; no se elimina un plato usado en pedidos |

## Correspondencia con la base de datos

- Los nombres siguen el DER en PascalCase: `ID_Cliente` → `IdCliente`, `Num_Mesa` → `NumMesa`, `Cod_Plato` → `CodPlato`, `Cant_Personas` → `CantPersonas`.
- `Total_Calc` es derivado: se calcula en `Pedido.Total` (suma de `Cantidad` × `Precio`) y no se guarda.
- `Estado` de Reserva y de Pedido se guarda como texto a partir de enumeraciones: `Pendiente`, `Confirmada`, `Completada`, `Cancelada` y `Abierto`, `Servido`, `Facturado`, `Cancelado`.
- Entidades y tablas: `Clientes`, `TelefonosCliente`, `Reservas`, `Mesas`, `Empleados`, `Pedidos`, `PedidoPlatos`, `Platos`, `Facturas`.
