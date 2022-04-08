using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddLoadout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineLoadouts",
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
                    LeftOver = table.Column<long>(nullable: true),
                    OutOfStock = table.Column<int>(nullable: false),
                    DispenseErrors = table.Column<int>(nullable: false),
                    Expired = table.Column<int>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "LoadoutItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LoadoutId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: true),
                    Price = table.Column<double>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    Capacity = table.Column<int>(nullable: false),
                    ItemLocation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadoutItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadoutItems_MachineLoadouts_LoadoutId",
                        column: x => x.LoadoutId,
                        principalTable: "MachineLoadouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadoutItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadoutItems_LoadoutId",
                table: "LoadoutItems",
                column: "LoadoutId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadoutItems_ProductId",
                table: "LoadoutItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineLoadouts_MachineId",
                table: "MachineLoadouts",
                column: "MachineId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadoutItems");

            migrationBuilder.DropTable(
                name: "MachineLoadouts");
        }
    }
}
