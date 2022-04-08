using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddRestockSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadoutItems_MachineLoadouts_LoadoutId",
                table: "LoadoutItems");

            migrationBuilder.DropTable(
                name: "MachineLoadouts");

            migrationBuilder.RenameColumn(
                name: "LoadoutId",
                table: "LoadoutItems",
                newName: "MachineId");

            migrationBuilder.RenameIndex(
                name: "IX_LoadoutItems_LoadoutId",
                table: "LoadoutItems",
                newName: "IX_LoadoutItems_MachineId");

            migrationBuilder.CreateTable(
                name: "RestockSessions",
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
                    MachineId = table.Column<Guid>(nullable: true),
                    Total = table.Column<int>(nullable: false),
                    LeftOver = table.Column<int>(nullable: false),
                    Sold = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestockSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestockSessions_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestockSessionHistory",
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
                    RestockSessionId = table.Column<Guid>(nullable: true),
                    LoadoutItemId = table.Column<Guid>(nullable: true),
                    OldProduct = table.Column<Guid>(nullable: true),
                    NewProduct = table.Column<Guid>(nullable: true),
                    PriceChange = table.Column<string>(nullable: true),
                    QuantityChange = table.Column<string>(nullable: true),
                    CapacityChange = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestockSessionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestockSessionHistory_LoadoutItems_LoadoutItemId",
                        column: x => x.LoadoutItemId,
                        principalTable: "LoadoutItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestockSessionHistory_RestockSessions_RestockSessionId",
                        column: x => x.RestockSessionId,
                        principalTable: "RestockSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestockSessionHistory_LoadoutItemId",
                table: "RestockSessionHistory",
                column: "LoadoutItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RestockSessionHistory_RestockSessionId",
                table: "RestockSessionHistory",
                column: "RestockSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestockSessions_MachineId",
                table: "RestockSessions",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadoutItems_Machines_MachineId",
                table: "LoadoutItems",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadoutItems_Machines_MachineId",
                table: "LoadoutItems");

            migrationBuilder.DropTable(
                name: "RestockSessionHistory");

            migrationBuilder.DropTable(
                name: "RestockSessions");

            migrationBuilder.RenameColumn(
                name: "MachineId",
                table: "LoadoutItems",
                newName: "LoadoutId");

            migrationBuilder.RenameIndex(
                name: "IX_LoadoutItems_MachineId",
                table: "LoadoutItems",
                newName: "IX_LoadoutItems_LoadoutId");

            migrationBuilder.CreateTable(
                name: "MachineLoadouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    DispenseErrors = table.Column<int>(nullable: false),
                    Expired = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LeftOver = table.Column<long>(nullable: true),
                    MachineId = table.Column<Guid>(nullable: true),
                    OutOfStock = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineLoadouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineLoadouts_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineLoadouts_MachineId",
                table: "MachineLoadouts",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadoutItems_MachineLoadouts_LoadoutId",
                table: "LoadoutItems",
                column: "LoadoutId",
                principalTable: "MachineLoadouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
