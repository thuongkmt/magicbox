using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AlterTableUser_AddPasscodeAndQRCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassCode",
                table: "AbpUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "AbpUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassCode",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "AbpUsers");
        }
    }
}
