using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AlterTopupToAddPreviousToupSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreviousTopupId",
                table: "Topups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topups_PreviousTopupId",
                table: "Topups",
                column: "PreviousTopupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topups_Topups_PreviousTopupId",
                table: "Topups",
                column: "PreviousTopupId",
                principalTable: "Topups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topups_Topups_PreviousTopupId",
                table: "Topups");

            migrationBuilder.DropIndex(
                name: "IX_Topups_PreviousTopupId",
                table: "Topups");

            migrationBuilder.DropColumn(
                name: "PreviousTopupId",
                table: "Topups");
        }
    }
}
