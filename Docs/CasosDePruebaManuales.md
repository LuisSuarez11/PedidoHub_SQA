# Casos de prueba manuales de PedidoHub

| | |
|---|---|
| **Sistema** | PedidoHub, ejecutado en local con SQL Server LocalDB (`http://localhost:5158`) |
| **Fecha de ejecución** | 30 de octubre de 2026 |
| **Técnica** | Caja negra: partición de equivalencia y valores límite; pruebas basadas en las historias de usuario |
| **Evidencia** | Una captura por caso en `Docs/evidencias/manuales/` (nombre del archivo indicado en cada caso) |
| **Resultado global** | 9 casos ejecutados, 9 aprobados, 0 defectos nuevos |

Las pruebas manuales complementan a las 216 automatizadas: verifican lo que se ve y se siente en pantalla (mensajes, ubicación de los errores, navegación por rol). Cada caso se puede repetir con los pasos indicados.

## Datos de partida
- Base de datos con los datos iniciales de la migración (5 mesas, 4 clientes, 3 empleados, 6 platos, 4 reservas, 5 pedidos, 4 facturas).
- Usuarios de demostración: `admin` (Administrador), `cajero` (Cajero), `mesero` (Mesero).

## Casos ejecutados

| ID | Historia | Precondición | Pasos | Resultado esperado | Resultado obtenido | Estado | Evidencia |
|---|---|---|---|---|---|---|---|
| CPM-01 | HU-16 | Sin sesión iniciada | 1. Abrir `http://localhost:5158/` | Se muestra la página pública con el titular, las funciones, los roles y los indicadores de calidad (216 pruebas, 100 %, 81.6 %, 6 / 6) | Se muestra la landing con los indicadores correctos | Aprobado | `CPM-01_landing_publica.jpg` |
| CPM-02 | HU-13 | Sin sesión iniciada | 1. Abrir Ingresar. 2. Usuario `admin` con una contraseña incorrecta. 3. Pulsar Ingresar | No se inicia sesión; mensaje genérico "Usuario o contraseña incorrectos."; el campo de contraseña queda vacío | Se mostró el mensaje esperado, sin revelar si el usuario existe, y la contraseña se limpió | Aprobado | `CPM-02_login_clave_incorrecta.jpg` |
| CPM-03 | HU-13, HU-01 | Sin sesión iniciada | 1. Abrir Ingresar. 2. Usuario `admin` con la contraseña correcta | Entra al Servicio: plano de mesas, "Por cobrar", próximas reservas, ventas de 30 días y menú completo de administrador | Se mostró el plano con 5 mesas (mesa 5 ocupada, 112.00 por cobrar), ventas de 603.00 y el menú con Panel de calidad y Bitácora | Aprobado | `CPM-03_login_admin_plano_de_mesas.jpg` |
| CPM-04 | HU-02 | Sesión de administrador; la mesa 5 tiene un pedido abierto | 1. Pedidos > Nuevo pedido. 2. Mesa 5, empleado Laura Gómez, estado Abierto. 3. Crear pedido | El sistema rechaza el pedido y muestra "La mesa 5 ya tiene un pedido sin facturar." junto al campo Mesa | Se mostró el mensaje esperado en el campo Mesa y no se creó el pedido | Aprobado | `CPM-04_mesa_ocupada_rechaza_pedido.jpg` |
| CPM-05 | HU-06 | Sesión de administrador | 1. Reservas > Nueva reserva. 2. Cliente Ana Torres, mesa 2, fecha 10/01/2026 20:00 (pasada), 2 personas. 3. Guardar | Se rechaza con "La fecha de una reserva nueva no puede estar en el pasado." | Se mostró el mensaje esperado bajo el campo Fecha y hora y los demás datos se conservaron | Aprobado | `CPM-05_reserva_en_el_pasado_rechazada.jpg` |
| CPM-06 | HU-08 | Sesión de administrador | 1. Platos > Nuevo plato. 2. Nombre "Majadito (prueba manual)", precio 32.50. 3. Guardar | El plato se crea, aparece en el menú con su precio y se muestra el aviso de éxito | Se creó, se listó con 32.50 y apareció el aviso "Plato registrado correctamente" | Aprobado | `CPM-06_plato_creado.jpg` |
| CPM-07 | HU-14 | CPM-06 ejecutado | 1. Abrir Bitácora | Aparece el movimiento del alta con fecha y hora, usuario `admin`, acción "Creó", entidad Plato y registro 7 | Se mostró un único movimiento con esos datos | Aprobado | `CPM-07_bitacora_registra_el_alta.jpg` |
| CPM-08 | HU-15 | Sesión de administrador | 1. Abrir Panel de calidad | Se muestran la tasa de éxito, la cobertura, los defectos corregidos, las historias hechas, el estado vivo del sistema, la pirámide de pruebas y el Definition of Done | Se mostraron todos los bloques; el estado vivo indicó base de datos disponible y 1 cambio auditado | Aprobado | `CPM-08_panel_de_calidad.jpg` |
| CPM-09 | HU-13 | Sesión cerrada y nueva sesión con el usuario `mesero` | 1. Cerrar sesión. 2. Ingresar como `mesero`. 3. Abrir `/Empleados` | Se redirige a "Acceso denegado" con el mensaje de que su rol no tiene permiso | Se mostró la pantalla "Acceso denegado" | Aprobado | `CPM-09_mesero_acceso_denegado.jpg` |

## Datos que dejó la ejecución
El caso CPM-06 creó el plato "Majadito (prueba manual)" en la base de datos local. Si se desea volver al estado inicial se elimina desde Platos (la bitácora conservará el alta y la baja) o se recrea la base con `dotnet ef database drop` y `dotnet ef database update`.

## Cómo repetir la ejecución
1. Iniciar la aplicación: `cd PedidoHub` y `dotnet run`.
2. Abrir la dirección que indique la consola y seguir los pasos de cada caso.
3. Anotar fecha, nombre de quien ejecuta y resultado en la copia de esta tabla.

| Repetición | Fecha | Ejecutó | Casos aprobados |
|---|---|---|---|
| 1 | | | / 9 |
