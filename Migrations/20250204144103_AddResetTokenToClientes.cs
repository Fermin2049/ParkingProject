using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalMarzo.net.Migrations
{
    /// <inheritdoc />
    public partial class AddResetTokenToClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "usuarios",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "clientes",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "clientes",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiry",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiry",
                table: "clientes");
        }
    }
}
