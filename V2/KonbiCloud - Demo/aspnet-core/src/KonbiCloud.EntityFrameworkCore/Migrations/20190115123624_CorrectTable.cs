using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class CorrectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishMachineSyncStatus_Discs_DiscId",
                table: "DishMachineSyncStatus");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "DishId",
                table: "DishMachineSyncStatus");

            migrationBuilder.AlterColumn<Guid>(
                name: "DiscId",
                table: "DishMachineSyncStatus",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DishMachineSyncStatus_Discs_DiscId",
                table: "DishMachineSyncStatus",
                column: "DiscId",
                principalTable: "Discs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishMachineSyncStatus_Discs_DiscId",
                table: "DishMachineSyncStatus");

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "Machines",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DiscId",
                table: "DishMachineSyncStatus",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "DishId",
                table: "DishMachineSyncStatus",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_DishMachineSyncStatus_Discs_DiscId",
                table: "DishMachineSyncStatus",
                column: "DiscId",
                principalTable: "Discs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
