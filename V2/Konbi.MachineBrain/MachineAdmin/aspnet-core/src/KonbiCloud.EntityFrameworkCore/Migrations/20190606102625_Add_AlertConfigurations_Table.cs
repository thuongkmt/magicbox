using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class Add_AlertConfigurations_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    ToEmail = table.Column<string>(nullable: false),
                    WhenChilledMachineTemperatureAbove = table.Column<int>(nullable: false),
                    WhenFrozenMachineTemperatureAbove = table.Column<int>(nullable: false),
                    WhenHotMachineTemperatureBelow = table.Column<int>(nullable: false),
                    SendEmailWhenProductExpiredDate = table.Column<int>(nullable: false),
                    WhenStockBellow = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertConfigurations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertConfigurations");
        }
    }
}
