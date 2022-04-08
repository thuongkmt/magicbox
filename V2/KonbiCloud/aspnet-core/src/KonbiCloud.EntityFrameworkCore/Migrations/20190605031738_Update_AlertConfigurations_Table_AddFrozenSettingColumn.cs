using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Update_AlertConfigurations_Table_AddFrozenSettingColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WhenFrozenMachineTemperatureAbove",
                table: "AlertConfigurations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhenFrozenMachineTemperatureAbove",
                table: "AlertConfigurations");
        }
    }
}
