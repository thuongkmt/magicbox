using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class UpdateProductTransaction1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransaction_Products_ProductId",
                table: "ProductTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransaction_Transactions_TransactionId",
                table: "ProductTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTransaction",
                table: "ProductTransaction");

            migrationBuilder.RenameTable(
                name: "ProductTransaction",
                newName: "ProductTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTransaction_TransactionId",
                table: "ProductTransactions",
                newName: "IX_ProductTransactions_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTransaction_ProductId",
                table: "ProductTransactions",
                newName: "IX_ProductTransactions_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTransactions",
                table: "ProductTransactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransactions_Products_ProductId",
                table: "ProductTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransactions_Transactions_TransactionId",
                table: "ProductTransactions",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransactions_Products_ProductId",
                table: "ProductTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransactions_Transactions_TransactionId",
                table: "ProductTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTransactions",
                table: "ProductTransactions");

            migrationBuilder.RenameTable(
                name: "ProductTransactions",
                newName: "ProductTransaction");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTransactions_TransactionId",
                table: "ProductTransaction",
                newName: "IX_ProductTransaction_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTransactions_ProductId",
                table: "ProductTransaction",
                newName: "IX_ProductTransaction_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTransaction",
                table: "ProductTransaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransaction_Products_ProductId",
                table: "ProductTransaction",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransaction_Transactions_TransactionId",
                table: "ProductTransaction",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
