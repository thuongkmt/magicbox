using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AlterTable_AlertConfiguration_ChangeColumnsToNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WhenStockBellow",
                table: "AlertConfigurations",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "WhenHotMachineTemperatureBelow",
                table: "AlertConfigurations",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "WhenFrozenMachineTemperatureAbove",
                table: "AlertConfigurations",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "WhenChilledMachineTemperatureAbove",
                table: "AlertConfigurations",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "ToEmail",
                table: "AlertConfigurations",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "SendEmailWhenProductExpiredDate",
                table: "AlertConfigurations",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WhenStockBellow",
                table: "AlertConfigurations",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WhenHotMachineTemperatureBelow",
                table: "AlertConfigurations",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WhenFrozenMachineTemperatureAbove",
                table: "AlertConfigurations",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WhenChilledMachineTemperatureAbove",
                table: "AlertConfigurations",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToEmail",
                table: "AlertConfigurations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SendEmailWhenProductExpiredDate",
                table: "AlertConfigurations",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
