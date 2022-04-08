using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class PlateMachineSyncStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "SyncDate",
                table: "Plates");

            migrationBuilder.CreateTable(
                name: "PlateMachineSyncStatus",
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
                    PlateId = table.Column<Guid>(nullable: false),
                    MachineId = table.Column<Guid>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateMachineSyncStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateMachineSyncStatus_Plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlateMachineSyncStatus_PlateId",
                table: "PlateMachineSyncStatus",
                column: "PlateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlateMachineSyncStatus");

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "Plates",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncDate",
                table: "Plates",
                nullable: true);
        }
    }
}
