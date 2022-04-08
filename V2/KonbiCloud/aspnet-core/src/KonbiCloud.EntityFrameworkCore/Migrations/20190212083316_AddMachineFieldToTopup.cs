using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddMachineFieldToTopup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MachineId",
                table: "Topups",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Topups_MachineId",
                table: "Topups",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topups_Machines_MachineId",
                table: "Topups",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topups_Machines_MachineId",
                table: "Topups");

            migrationBuilder.DropIndex(
                name: "IX_Topups_MachineId",
                table: "Topups");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "Topups");
        }
    }
}
