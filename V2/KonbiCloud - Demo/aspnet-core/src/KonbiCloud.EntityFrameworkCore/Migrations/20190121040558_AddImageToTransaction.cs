using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddImageToTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BeginTranImage",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTranImage",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranVideo",
                table: "Transactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeginTranImage",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EndTranImage",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TranVideo",
                table: "Transactions");
        }
    }
}
