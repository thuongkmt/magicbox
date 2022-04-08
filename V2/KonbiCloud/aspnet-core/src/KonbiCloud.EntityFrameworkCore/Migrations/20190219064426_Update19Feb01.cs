using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Update19Feb01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeftOver",
                table: "Topups");

            migrationBuilder.AddColumn<long>(
                name: "DetailTransactionId",
                table: "Inventories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_DetailTransactionId",
                table: "Inventories",
                column: "DetailTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Transactions_DetailTransactionId",
                table: "Inventories",
                column: "DetailTransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Transactions_DetailTransactionId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_DetailTransactionId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DetailTransactionId",
                table: "Inventories");

            migrationBuilder.AddColumn<int>(
                name: "LeftOver",
                table: "Topups",
                nullable: false,
                defaultValue: 0);
        }
    }
}
