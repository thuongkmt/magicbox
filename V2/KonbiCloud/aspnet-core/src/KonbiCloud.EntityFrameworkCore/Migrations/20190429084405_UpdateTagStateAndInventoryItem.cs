using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class UpdateTagStateAndInventoryItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Topups_TopupId",
                table: "Inventories");

            migrationBuilder.AlterColumn<Guid>(
                name: "TopupId",
                table: "Inventories",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Topups_TopupId",
                table: "Inventories",
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

            migrationBuilder.AlterColumn<Guid>(
                name: "TopupId",
                table: "Inventories",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Topups_TopupId",
                table: "Inventories",
                column: "TopupId",
                principalTable: "Topups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
