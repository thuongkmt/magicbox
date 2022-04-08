using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MachineId",
                table: "Inventories",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_MachineId",
                table: "Inventories",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Machines_MachineId",
                table: "Inventories",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Machines_MachineId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_MachineId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "Inventories");
        }
    }
}
