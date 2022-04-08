using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddInventoryToDetailTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DetailTransaction",
                table: "Inventories",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TransactionId",
                table: "Inventories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_TransactionId",
                table: "Inventories",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Transactions_TransactionId",
                table: "Inventories",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Transactions_TransactionId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_TransactionId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DetailTransaction",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Inventories");
        }
    }
}
