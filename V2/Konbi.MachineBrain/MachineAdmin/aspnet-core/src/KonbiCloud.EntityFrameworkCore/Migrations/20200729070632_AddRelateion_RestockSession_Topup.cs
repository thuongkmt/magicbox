using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddRelateion_RestockSession_Topup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestockSessionId",
                table: "Topups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topups_RestockSessionId",
                table: "Topups",
                column: "RestockSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topups_RestockSessions_RestockSessionId",
                table: "Topups",
                column: "RestockSessionId",
                principalTable: "RestockSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topups_RestockSessions_RestockSessionId",
                table: "Topups");

            migrationBuilder.DropIndex(
                name: "IX_Topups_RestockSessionId",
                table: "Topups");

            migrationBuilder.DropColumn(
                name: "RestockSessionId",
                table: "Topups");
        }
    }
}
