using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendORCinverisones.Migrations
{
    /// <inheritdoc />
    public partial class AddDescripcionFichaTecnicaToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FichaTecnica",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 16, 33, 37, 143, DateTimeKind.Local).AddTicks(7552));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 16, 33, 37, 143, DateTimeKind.Local).AddTicks(7563));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FichaTecnica",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 14, 0, 16, 176, DateTimeKind.Local).AddTicks(2569));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 14, 0, 16, 176, DateTimeKind.Local).AddTicks(2579));
        }
    }
}
