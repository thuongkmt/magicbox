using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Update19Feb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Transactions_TransactionId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DetailTransaction",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Inventories",
                newName: "DetailTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_TransactionId",
                table: "Inventories",
                newName: "IX_Inventories_DetailTransactionId");

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

            migrationBuilder.RenameColumn(
                name: "DetailTransactionId",
                table: "Inventories",
                newName: "TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_DetailTransactionId",
                table: "Inventories",
                newName: "IX_Inventories_TransactionId");

            migrationBuilder.AddColumn<long>(
                name: "DetailTransaction",
                table: "Inventories",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Transactions_TransactionId",
                table: "Inventories",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
