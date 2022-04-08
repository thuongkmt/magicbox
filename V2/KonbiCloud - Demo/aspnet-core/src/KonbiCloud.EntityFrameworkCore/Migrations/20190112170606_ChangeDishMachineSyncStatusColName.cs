using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class ChangeDishMachineSyncStatusColName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlateId",
                table: "DishMachineSyncStatus",
                newName: "DishId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DishId",
                table: "DishMachineSyncStatus",
                newName: "PlateId");
        }
    }
}
