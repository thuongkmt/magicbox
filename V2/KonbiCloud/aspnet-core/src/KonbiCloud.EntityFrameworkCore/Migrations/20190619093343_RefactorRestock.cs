using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class RefactorRestock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestockSessionHistory_RestockSessions_RestockSessionId",
                table: "RestockSessionHistory");

            migrationBuilder.DropTable(
                name: "RestockSessions");

            migrationBuilder.AddColumn<bool>(
                name: "IsInprogress",
                table: "Topups",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LeftOver",
                table: "Topups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_RestockSessionHistory_Topups_RestockSessionId",
                table: "RestockSessionHistory",
                column: "RestockSessionId",
                principalTable: "Topups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestockSessionHistory_Topups_RestockSessionId",
                table: "RestockSessionHistory");

            migrationBuilder.DropColumn(
                name: "IsInprogress",
                table: "Topups");

            migrationBuilder.DropColumn(
                name: "LeftOver",
                table: "Topups");

            migrationBuilder.CreateTable(
                name: "RestockSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsClose = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsInprogress = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LeftOver = table.Column<int>(nullable: false),
                    MachineId = table.Column<Guid>(nullable: false),
                    Sold = table.Column<int>(nullable: false),
                    Total = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestockSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestockSessions_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestockSessions_MachineId",
                table: "RestockSessions",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestockSessionHistory_RestockSessions_RestockSessionId",
                table: "RestockSessionHistory",
                column: "RestockSessionId",
                principalTable: "RestockSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
