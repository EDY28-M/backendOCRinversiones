using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendORCinverisones.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Codigo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CodigoComer",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 2, 29, 19, 473, DateTimeKind.Local).AddTicks(9320));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 2, 29, 19, 473, DateTimeKind.Local).AddTicks(9337));

            migrationBuilder.CreateIndex(
                name: "IX_Products_Codigo",
                table: "Products",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CodigoComer",
                table: "Products",
                column: "CodigoComer",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_CategoryId",
                table: "Products",
                columns: new[] { "IsActive", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_CreatedAt",
                table: "Products",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_MarcaId",
                table: "Products",
                columns: new[] { "IsActive", "MarcaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured_IsActive_CreatedAt",
                table: "Products",
                columns: new[] { "IsFeatured", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Users_Roles_RoleId",
            //     table: "Users",
            //     column: "RoleId",
            //     principalTable: "Roles",
            //     principalColumn: "Id",
            //     onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Users_Roles_RoleId",
            //     table: "Users");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_Products_Codigo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CodigoComer",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_MarcaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsFeatured_IsActive_CreatedAt",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 23, 59, 0, 418, DateTimeKind.Local).AddTicks(4532));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 23, 23, 59, 0, 418, DateTimeKind.Local).AddTicks(4540));

            migrationBuilder.CreateIndex(
                name: "IX_Products_Codigo",
                table: "Products",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CodigoComer",
                table: "Products",
                column: "CodigoComer");
        }
    }
}
