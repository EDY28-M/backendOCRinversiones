using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendORCinverisones.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductModelWithNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "CodigoPD");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "Descripcion");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Name",
                table: "Products",
                newName: "IX_Products_CodigoPD");

            migrationBuilder.AddColumn<string>(
                name: "CodigoOEM",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FichaTecnica",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imagen2",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imagen3",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imagen4",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenPrincipal",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 9, 40, 37, 948, DateTimeKind.Local).AddTicks(6262));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 9, 40, 37, 948, DateTimeKind.Local).AddTicks(6274));

            migrationBuilder.CreateIndex(
                name: "IX_Products_CodigoOEM",
                table: "Products",
                column: "CodigoOEM");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CodigoOEM",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CodigoOEM",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FichaTecnica",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Imagen2",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Imagen3",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Imagen4",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImagenPrincipal",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Products",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CodigoPD",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CodigoPD",
                table: "Products",
                newName: "IX_Products_Name");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 22, 16, 45, 14, 339, DateTimeKind.Local).AddTicks(472));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 22, 16, 45, 14, 339, DateTimeKind.Local).AddTicks(482));
        }
    }
}
