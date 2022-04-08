using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddIsClose_RestockSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestockSessions_Machines_MachineId",
                table: "RestockSessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "MachineId",
                table: "RestockSessions",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClose",
                table: "RestockSessions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInprogress",
                table: "RestockSessions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_RestockSessions_Machines_MachineId",
                table: "RestockSessions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestockSessions_Machines_MachineId",
                table: "RestockSessions");

            migrationBuilder.DropColumn(
                name: "IsClose",
                table: "RestockSessions");

            migrationBuilder.DropColumn(
                name: "IsInprogress",
                table: "RestockSessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "MachineId",
                table: "RestockSessions",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_RestockSessions_Machines_MachineId",
                table: "RestockSessions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
