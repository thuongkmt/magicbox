using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class ChangeColumnClientLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Transactions");

            migrationBuilder.AddColumn<long>(
                name: "LocalTranId",
                table: "Transactions",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalTranId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Transactions",
                nullable: true);
        }
    }
}
