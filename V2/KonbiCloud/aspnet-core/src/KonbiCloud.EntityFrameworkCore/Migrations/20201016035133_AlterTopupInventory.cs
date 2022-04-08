using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AlterTopupInventory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "TopupInventory",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "TopupInventory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "TopupInventory",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TopupInventory",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "TopupInventory");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "TopupInventory");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "TopupInventory");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TopupInventory");
        }
    }
}
