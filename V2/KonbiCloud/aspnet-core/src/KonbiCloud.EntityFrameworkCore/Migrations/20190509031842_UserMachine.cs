using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class UserMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadoutItems");

            migrationBuilder.CreateTable(
                name: "UserMachines",
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
                    UserId = table.Column<long>(nullable: false),
                    MachineId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMachines_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMachines_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_MachineId",
                table: "UserMachines",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_UserId",
                table: "UserMachines",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMachines");

            migrationBuilder.CreateTable(
                name: "LoadoutItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    DocumentDbProductId = table.Column<string>(nullable: true),
                    IsEnable = table.Column<bool>(nullable: false),
                    LocalProductId = table.Column<long>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    ProductName = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadoutItems", x => x.Id);
                });
        }
    }
}
