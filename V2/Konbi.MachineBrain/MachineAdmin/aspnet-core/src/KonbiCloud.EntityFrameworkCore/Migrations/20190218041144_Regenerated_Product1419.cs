using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Regenerated_Product1419 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SyncDate",
                table: "Products");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "Products",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductCategoryId",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncDate",
                table: "Products",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
