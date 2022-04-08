using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddRefRsMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MachineId",
                table: "RestockSessions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestockSessions_MachineId",
                table: "RestockSessions",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestockSessions_Machines_MachineId",
                table: "RestockSessions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestockSessions_Machines_MachineId",
                table: "RestockSessions");

            migrationBuilder.DropIndex(
                name: "IX_RestockSessions_MachineId",
                table: "RestockSessions");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "RestockSessions");
        }
    }
}
