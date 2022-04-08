using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddDishMachineSyncStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "Discs");

            migrationBuilder.DropColumn(
                name: "SyncDate",
                table: "Discs");

            migrationBuilder.CreateTable(
                name: "DishMachineSyncStatus",
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
                    SyncDate = table.Column<DateTime>(nullable: true),
                    DiscId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishMachineSyncStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishMachineSyncStatus_Discs_DiscId",
                        column: x => x.DiscId,
                        principalTable: "Discs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DishMachineSyncStatus_DiscId",
                table: "DishMachineSyncStatus",
                column: "DiscId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishMachineSyncStatus");

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "Discs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncDate",
                table: "Discs",
                nullable: true);
        }
    }
}
