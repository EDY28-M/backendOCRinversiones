using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendORCinverisones.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Producto",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Producto",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 13, 14, 38, 927, DateTimeKind.Local).AddTicks(3760));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 13, 14, 38, 927, DateTimeKind.Local).AddTicks(3770));
        }
    }
}
