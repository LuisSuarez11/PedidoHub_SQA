using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PedidoHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class CrearModeloPedidoHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IdCliente);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    IdEmpleado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.IdEmpleado);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    NumMesa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Capacidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.NumMesa);
                    table.CheckConstraint("CK_Mesas_Capacidad", "[Capacidad] BETWEEN 1 AND 20");
                });

            migrationBuilder.CreateTable(
                name: "Platos",
                columns: table => new
                {
                    CodPlato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platos", x => x.CodPlato);
                    table.CheckConstraint("CK_Platos_Precio", "[Precio] > 0");
                });

            migrationBuilder.CreateTable(
                name: "TelefonosCliente",
                columns: table => new
                {
                    IdTelefono = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelefonosCliente", x => x.IdTelefono);
                    table.ForeignKey(
                        name: "FK_TelefonosCliente_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    IdPedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumMesa = table.Column<int>(type: "int", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.IdPedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Empleados_IdEmpleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "IdEmpleado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Mesas_NumMesa",
                        column: x => x.NumMesa,
                        principalTable: "Mesas",
                        principalColumn: "NumMesa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    IdReserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantPersonas = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    NumMesa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.IdReserva);
                    table.CheckConstraint("CK_Reservas_CantPersonas", "[CantPersonas] > 0");
                    table.ForeignKey(
                        name: "FK_Reservas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Mesas_NumMesa",
                        column: x => x.NumMesa,
                        principalTable: "Mesas",
                        principalColumn: "NumMesa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    NumFact = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NIT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdPedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.NumFact);
                    table.ForeignKey(
                        name: "FK_Facturas_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoPlatos",
                columns: table => new
                {
                    IdPedido = table.Column<int>(type: "int", nullable: false),
                    CodPlato = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoPlatos", x => new { x.IdPedido, x.CodPlato });
                    table.CheckConstraint("CK_PedidoPlatos_Cantidad", "[Cantidad] > 0");
                    table.ForeignKey(
                        name: "FK_PedidoPlatos_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoPlatos_Platos_CodPlato",
                        column: x => x.CodPlato,
                        principalTable: "Platos",
                        principalColumn: "CodPlato",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "IdCliente", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ana Torres" },
                    { 2, "Bruno Díaz" },
                    { 3, "Carla Méndez" },
                    { 4, "Diego Ruiz" }
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "IdEmpleado", "Cargo", "Nombre" },
                values: new object[,]
                {
                    { 1, "Mesera", "Laura Gómez" },
                    { 2, "Mesero", "Pedro Salas" },
                    { 3, "Cajera", "Marta Rojas" }
                });

            migrationBuilder.InsertData(
                table: "Mesas",
                columns: new[] { "NumMesa", "Capacidad" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 2, 4 },
                    { 3, 4 },
                    { 4, 6 },
                    { 5, 8 }
                });

            migrationBuilder.InsertData(
                table: "Platos",
                columns: new[] { "CodPlato", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, "Pique Macho", 55m },
                    { 2, "Silpancho", 40m },
                    { 3, "Sopa de Maní", 25m },
                    { 4, "Salteña", 12m },
                    { 5, "Refresco", 10m },
                    { 6, "Flan", 18m }
                });

            migrationBuilder.InsertData(
                table: "Pedidos",
                columns: new[] { "IdPedido", "Estado", "FechaHora", "IdEmpleado", "NumMesa" },
                values: new object[,]
                {
                    { 1, "Facturado", new DateTime(2026, 10, 3, 13, 15, 0, 0, DateTimeKind.Unspecified), 1, 1 },
                    { 2, "Facturado", new DateTime(2026, 10, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), 2, 2 },
                    { 3, "Facturado", new DateTime(2026, 10, 17, 20, 30, 0, 0, DateTimeKind.Unspecified), 1, 4 },
                    { 4, "Facturado", new DateTime(2026, 10, 24, 21, 0, 0, 0, DateTimeKind.Unspecified), 2, 3 },
                    { 5, "Abierto", new DateTime(2026, 10, 25, 12, 45, 0, 0, DateTimeKind.Unspecified), 1, 5 }
                });

            migrationBuilder.InsertData(
                table: "Reservas",
                columns: new[] { "IdReserva", "CantPersonas", "Estado", "FechaHora", "IdCliente", "NumMesa" },
                values: new object[,]
                {
                    { 1, 2, "Completada", new DateTime(2026, 10, 2, 20, 0, 0, 0, DateTimeKind.Unspecified), 1, 1 },
                    { 2, 4, "Completada", new DateTime(2026, 10, 9, 13, 0, 0, 0, DateTimeKind.Unspecified), 2, 2 },
                    { 3, 6, "Confirmada", new DateTime(2026, 10, 30, 21, 0, 0, 0, DateTimeKind.Unspecified), 3, 4 },
                    { 4, 3, "Pendiente", new DateTime(2026, 11, 2, 12, 30, 0, 0, DateTimeKind.Unspecified), 4, 3 }
                });

            migrationBuilder.InsertData(
                table: "TelefonosCliente",
                columns: new[] { "IdTelefono", "IdCliente", "Numero" },
                values: new object[,]
                {
                    { 1, 1, "71234567" },
                    { 2, 1, "69871122" },
                    { 3, 2, "70001234" },
                    { 4, 3, "76543210" },
                    { 5, 4, "78889999" }
                });

            migrationBuilder.InsertData(
                table: "Facturas",
                columns: new[] { "NumFact", "FechaEmision", "IdPedido", "NIT" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 3, 14, 30, 0, 0, DateTimeKind.Unspecified), 1, "1020304011" },
                    { 2, new DateTime(2026, 10, 10, 15, 10, 0, 0, DateTimeKind.Unspecified), 2, "4455667788" },
                    { 3, new DateTime(2026, 10, 17, 22, 0, 0, 0, DateTimeKind.Unspecified), 3, "9988776655" },
                    { 4, new DateTime(2026, 10, 24, 22, 15, 0, 0, DateTimeKind.Unspecified), 4, "1020304011" }
                });

            migrationBuilder.InsertData(
                table: "PedidoPlatos",
                columns: new[] { "CodPlato", "IdPedido", "Cantidad" },
                values: new object[,]
                {
                    { 1, 1, 2 },
                    { 5, 1, 2 },
                    { 2, 2, 3 },
                    { 4, 2, 3 },
                    { 5, 2, 3 },
                    { 3, 3, 4 },
                    { 6, 3, 4 },
                    { 1, 4, 1 },
                    { 2, 4, 1 },
                    { 5, 4, 2 },
                    { 4, 5, 6 },
                    { 5, 5, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdPedido",
                table: "Facturas",
                column: "IdPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoPlatos_CodPlato",
                table: "PedidoPlatos",
                column: "CodPlato");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdEmpleado",
                table: "Pedidos",
                column: "IdEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_NumMesa",
                table: "Pedidos",
                column: "NumMesa");

            migrationBuilder.CreateIndex(
                name: "IX_Platos_Nombre",
                table: "Platos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdCliente",
                table: "Reservas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_NumMesa",
                table: "Reservas",
                column: "NumMesa");

            migrationBuilder.CreateIndex(
                name: "IX_TelefonosCliente_IdCliente",
                table: "TelefonosCliente",
                column: "IdCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "PedidoPlatos");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "TelefonosCliente");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Platos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "Mesas");
        }
    }
}
