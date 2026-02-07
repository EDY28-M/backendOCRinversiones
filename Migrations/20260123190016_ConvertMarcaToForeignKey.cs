using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendORCinverisones.Migrations
{
    /// <inheritdoc />
    public partial class ConvertMarcaToForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear una marca por defecto "Sin Marca"
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM NombreMarcas WHERE Nombre = 'Sin Marca')
                BEGIN
                    INSERT INTO NombreMarcas (Nombre, IsActive, CreatedAt, UpdatedAt)
                    VALUES ('Sin Marca', 1, GETUTCDATE(), GETUTCDATE())
                END
            ");

            // 2. Agregar columna MarcaId (nullable temporalmente)
            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "Products",
                type: "int",
                nullable: true);

            // 3. Asignar marca "Sin Marca" a todos los productos existentes
            migrationBuilder.Sql(@"
                UPDATE Products 
                SET MarcaId = (SELECT TOP 1 Id FROM NombreMarcas WHERE Nombre = 'Sin Marca')
                WHERE MarcaId IS NULL
            ");

            // 4. Hacer MarcaId NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 5. Eliminar columna Marca
            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Products");

            // 6. Actualizar roles (seed data)
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

            // 7. Crear índice
            migrationBuilder.CreateIndex(
                name: "IX_Products_MarcaId",
                table: "Products",
                column: "MarcaId");

            // 8. Agregar FK constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Products_NombreMarcas_MarcaId",
                table: "Products",
                column: "MarcaId",
                principalTable: "NombreMarcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_NombreMarcas_MarcaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_MarcaId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 13, 29, 4, 312, DateTimeKind.Local).AddTicks(4503));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 13, 29, 4, 312, DateTimeKind.Local).AddTicks(4515));
        }
    }
}
