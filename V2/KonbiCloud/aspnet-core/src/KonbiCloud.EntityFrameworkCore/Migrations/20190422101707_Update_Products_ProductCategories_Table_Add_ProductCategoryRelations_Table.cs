using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Update_Products_ProductCategories_Table_Add_ProductCategoryRelations_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortDesc3",
                table: "Products",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "ShortDesc2",
                table: "Products",
                newName: "ShortDesc");

            migrationBuilder.RenameColumn(
                name: "ShortDesc1",
                table: "Products",
                newName: "Barcode");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "ProductCategories",
                newName: "Desc");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProductCategories",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategoryRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductCategoryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategoryRelations_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategoryRelations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryRelations_ProductCategoryId",
                table: "ProductCategoryRelations",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryRelations_ProductId",
                table: "ProductCategoryRelations",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategoryRelations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductCategories");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "Products",
                newName: "ShortDesc3");

            migrationBuilder.RenameColumn(
                name: "ShortDesc",
                table: "Products",
                newName: "ShortDesc2");

            migrationBuilder.RenameColumn(
                name: "Barcode",
                table: "Products",
                newName: "ShortDesc1");

            migrationBuilder.RenameColumn(
                name: "Desc",
                table: "ProductCategories",
                newName: "ImageUrl");
        }
    }
}
