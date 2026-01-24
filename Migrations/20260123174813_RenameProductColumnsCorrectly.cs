using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendORCinverisones.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductColumnsCorrectly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renombrar CodigoOEM -> Codigo
            migrationBuilder.RenameColumn(
                name: "CodigoOEM",
                table: "Products",
                newName: "Codigo");

            // Renombrar CodigoPD -> CodigoComer
            migrationBuilder.RenameColumn(
                name: "CodigoPD",
                table: "Products",
                newName: "CodigoComer");

            // Renombrar Descripcion -> Marca
            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Products",
                newName: "Marca");

            // Eliminar FichaTecnica
            migrationBuilder.DropColumn(
                name: "FichaTecnica",
                table: "Products");

            // Renombrar índices
            migrationBuilder.RenameIndex(
                name: "IX_Products_CodigoOEM",
                table: "Products",
                newName: "IX_Products_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CodigoPD",
                table: "Products",
                newName: "IX_Products_CodigoComer");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 12, 48, 13, 203, DateTimeKind.Local).AddTicks(5158));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 12, 48, 13, 203, DateTimeKind.Local).AddTicks(5172));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar FichaTecnica
            migrationBuilder.AddColumn<string>(
                name: "FichaTecnica",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Renombrar Marca -> Descripcion
            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "Products",
                newName: "Descripcion");

            // Renombrar CodigoComer -> CodigoPD
            migrationBuilder.RenameColumn(
                name: "CodigoComer",
                table: "Products",
                newName: "CodigoPD");

            // Renombrar Codigo -> CodigoOEM
            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "Products",
                newName: "CodigoOEM");

            // Restaurar índices
            migrationBuilder.RenameIndex(
                name: "IX_Products_CodigoComer",
                table: "Products",
                newName: "IX_Products_CodigoPD");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Codigo",
                table: "Products",
                newName: "IX_Products_CodigoOEM");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 12, 43, 4, 228, DateTimeKind.Local).AddTicks(9636));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 12, 43, 4, 228, DateTimeKind.Local).AddTicks(9656));
        }
    }
}
