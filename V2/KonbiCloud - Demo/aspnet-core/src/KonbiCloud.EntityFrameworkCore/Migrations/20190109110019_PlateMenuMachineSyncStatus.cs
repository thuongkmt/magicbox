using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class PlateMenuMachineSyncStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "PlateMenus");

            migrationBuilder.DropColumn(
                name: "SyncDate",
                table: "PlateMenus");

            migrationBuilder.CreateTable(
                name: "PlateMenuMachineSyncStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    PlateMenuId = table.Column<Guid>(nullable: false),
                    MachineId = table.Column<Guid>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateMenuMachineSyncStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateMenuMachineSyncStatus_PlateMenus_PlateMenuId",
                        column: x => x.PlateMenuId,
                        principalTable: "PlateMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenuMachineSyncStatus_PlateMenuId",
                table: "PlateMenuMachineSyncStatus",
                column: "PlateMenuId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlateMenuMachineSyncStatus");

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "PlateMenus",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncDate",
                table: "PlateMenus",
                nullable: true);
        }
    }
}
