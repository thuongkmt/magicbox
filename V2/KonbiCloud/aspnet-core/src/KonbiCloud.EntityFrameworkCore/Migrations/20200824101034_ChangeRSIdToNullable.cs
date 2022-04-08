using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class ChangeRSIdToNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topups_RestockSessions_RestockSessionId",
                table: "Topups");

            migrationBuilder.AlterColumn<int>(
                name: "RestockSessionId",
                table: "Topups",
                nullable: true,
                oldClrType: typeof(int));

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

            migrationBuilder.AlterColumn<int>(
                name: "RestockSessionId",
                table: "Topups",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Topups_RestockSessions_RestockSessionId",
                table: "Topups",
                column: "RestockSessionId",
                principalTable: "RestockSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
