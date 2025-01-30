using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalMarzo.net.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    idCliente = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vehiculoPlaca = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaRegistro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()"),
                    Password = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idCliente);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "espaciosestacionamiento",
                columns: table => new
                {
                    idEspacio = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    numeroEspacio = table.Column<int>(type: "int(11)", nullable: false),
                    estado = table.Column<string>(type: "enum('Disponible','Ocupado','Reservado')", nullable: true, defaultValueSql: "'Disponible'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipoEspacio = table.Column<string>(type: "enum('Normal','Discapacitados')", nullable: true, defaultValueSql: "'Normal'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sector = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idEspacio);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    idUsuario = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrasena = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rol = table.Column<string>(type: "enum('Administrador','Empleado')", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaRegistro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()"),
                    estado = table.Column<string>(type: "enum('Activo','Inactivo')", nullable: true, defaultValueSql: "'Activo'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idUsuario);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    idPago = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idCliente = table.Column<int>(type: "int(11)", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    metodoPago = table.Column<string>(type: "enum('Efectivo','Tarjeta','Transferencia')", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fechaPago = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()"),
                    estado = table.Column<string>(type: "enum('Exitoso','Fallido')", nullable: true, defaultValueSql: "'Exitoso'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idPago);
                    table.ForeignKey(
                        name: "pagos_ibfk_1",
                        column: x => x.idCliente,
                        principalTable: "clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "historialestacionamiento",
                columns: table => new
                {
                    idHistorial = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idCliente = table.Column<int>(type: "int(11)", nullable: false),
                    idEspacio = table.Column<int>(type: "int(11)", nullable: false),
                    fechaEntrada = table.Column<DateTime>(type: "datetime", nullable: false),
                    fechaSalida = table.Column<DateTime>(type: "datetime", nullable: true),
                    estado = table.Column<string>(type: "enum('Activo','Completado')", nullable: true, defaultValueSql: "'Activo'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idHistorial);
                    table.ForeignKey(
                        name: "historialestacionamiento_ibfk_1",
                        column: x => x.idCliente,
                        principalTable: "clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "historialestacionamiento_ibfk_2",
                        column: x => x.idEspacio,
                        principalTable: "espaciosestacionamiento",
                        principalColumn: "idEspacio",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    idReserva = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idCliente = table.Column<int>(type: "int(11)", nullable: false),
                    idEspacio = table.Column<int>(type: "int(11)", nullable: false),
                    fechaReserva = table.Column<DateTime>(type: "datetime", nullable: false),
                    fechaExpiracion = table.Column<DateTime>(type: "datetime", nullable: false),
                    estado = table.Column<string>(type: "enum('Activa','Cancelada','Expirada')", nullable: true, defaultValueSql: "'Activa'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idReserva);
                    table.ForeignKey(
                        name: "reservas_ibfk_1",
                        column: x => x.idCliente,
                        principalTable: "clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "reservas_ibfk_2",
                        column: x => x.idEspacio,
                        principalTable: "espaciosestacionamiento",
                        principalColumn: "idEspacio",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "email",
                table: "clientes",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "telefono",
                table: "clientes",
                column: "telefono",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "vehiculoPlaca",
                table: "clientes",
                column: "vehiculoPlaca",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "numeroEspacio",
                table: "espaciosestacionamiento",
                column: "numeroEspacio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idCliente",
                table: "historialestacionamiento",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "idEspacio",
                table: "historialestacionamiento",
                column: "idEspacio");

            migrationBuilder.CreateIndex(
                name: "idCliente1",
                table: "pagos",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "idCliente2",
                table: "reservas",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "idEspacio1",
                table: "reservas",
                column: "idEspacio");

            migrationBuilder.CreateIndex(
                name: "email1",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historialestacionamiento");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "espaciosestacionamiento");
        }
    }
}
