using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class RestockSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestockSessions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Total = table.Column<int>(nullable: false),
                    LeftOver = table.Column<int>(nullable: false),
                    Sold = table.Column<int>(nullable: false),
                    Error = table.Column<int>(nullable: false),
                    IsProcessing = table.Column<bool>(nullable: false),
                    RestockerName = table.Column<string>(nullable: true),
                    Restocked = table.Column<int>(nullable: false),
                    Unloaded = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestockSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestockSessions_TenantId",
                table: "RestockSessions",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestockSessions");
        }
    }
}
