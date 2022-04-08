using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Add_CashlessDetails_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CashlessDetail_CashlessDetailId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CashlessDetail",
                table: "CashlessDetail");

            migrationBuilder.RenameTable(
                name: "CashlessDetail",
                newName: "CashlessDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashlessDetails",
                table: "CashlessDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CashlessDetails_CashlessDetailId",
                table: "Transactions",
                column: "CashlessDetailId",
                principalTable: "CashlessDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CashlessDetails_CashlessDetailId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CashlessDetails",
                table: "CashlessDetails");

            migrationBuilder.RenameTable(
                name: "CashlessDetails",
                newName: "CashlessDetail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashlessDetail",
                table: "CashlessDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CashlessDetail_CashlessDetailId",
                table: "Transactions",
                column: "CashlessDetailId",
                principalTable: "CashlessDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
