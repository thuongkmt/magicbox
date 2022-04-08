using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class ModifyInventoryRfidFridge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TopupId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TopupId",
                table: "Inventories",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Topups",
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
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Total = table.Column<int>(nullable: false),
                    LeftOver = table.Column<int>(nullable: false),
                    Sold = table.Column<int>(nullable: false),
                    Error = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TopupId",
                table: "Transactions",
                column: "TopupId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_TopupId",
                table: "Inventories",
                column: "TopupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Topups_TopupId",
                table: "Inventories",
                column: "TopupId",
                principalTable: "Topups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Topups_TopupId",
                table: "Transactions",
                column: "TopupId",
                principalTable: "Topups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Topups_TopupId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Topups_TopupId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Topups");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TopupId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_TopupId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "TopupId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TopupId",
                table: "Inventories");
        }
    }
}
