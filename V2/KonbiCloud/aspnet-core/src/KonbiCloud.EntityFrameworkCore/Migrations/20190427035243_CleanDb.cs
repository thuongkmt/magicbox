using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class CleanDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceStrategies_PlateMenus_PlateMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropTable(
                name: "DishMachineSyncStatus");

            migrationBuilder.DropTable(
                name: "DishTransaction");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "PlateMachineSyncStatus");

            migrationBuilder.DropTable(
                name: "PlateMenuMachineSyncStatus");

            migrationBuilder.DropTable(
                name: "Trays");

            migrationBuilder.DropTable(
                name: "Discs");

            migrationBuilder.DropTable(
                name: "PlateMenus");

            migrationBuilder.DropTable(
                name: "Plates");

            migrationBuilder.DropTable(
                name: "PlateCategories");

            migrationBuilder.DropIndex(
                name: "IX_PriceStrategies_PlateMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropColumn(
                name: "BeginTranImage",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "EndTranImage",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TranVideo",
                table: "Transactions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BeginTranImage",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTranImage",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranVideo",
                table: "Transactions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CardId = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Ordered = table.Column<double>(nullable: false),
                    Period = table.Column<string>(nullable: true),
                    Quota = table.Column<double>(nullable: false),
                    QuotaCash = table.Column<bool>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlateCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    Desc = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trays",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Avaiable = table.Column<int>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    Color = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    Desc = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PlateCategoryId = table.Column<int>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plates_PlateCategories_PlateCategoryId",
                        column: x => x.PlateCategoryId,
                        principalTable: "PlateCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Discs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    PlateId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    Uid = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discs_Plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlateMachineSyncStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    MachineId = table.Column<Guid>(nullable: false),
                    PlateId = table.Column<Guid>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
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

            migrationBuilder.CreateTable(
                name: "PlateMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContractorPrice = table.Column<decimal>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    PlateId = table.Column<Guid>(nullable: false),
                    Price = table.Column<decimal>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: true),
                    SelectedDate = table.Column<DateTime>(nullable: false),
                    SessionId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateMenus_Plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlateMenus_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlateMenus_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DishMachineSyncStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    DiscId = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    MachineId = table.Column<Guid>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishMachineSyncStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishMachineSyncStatus_Discs_DiscId",
                        column: x => x.DiscId,
                        principalTable: "Discs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DishTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DiscId = table.Column<Guid>(nullable: true),
                    TransactionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishTransaction_Discs_DiscId",
                        column: x => x.DiscId,
                        principalTable: "Discs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DishTransaction_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlateMenuMachineSyncStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    MachineId = table.Column<Guid>(nullable: false),
                    PlateMenuId = table.Column<Guid>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
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
                name: "IX_PriceStrategies_PlateMenuId",
                table: "PriceStrategies",
                column: "PlateMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Discs_PlateId",
                table: "Discs",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_Discs_TenantId",
                table: "Discs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DishMachineSyncStatus_DiscId",
                table: "DishMachineSyncStatus",
                column: "DiscId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTransaction_DiscId",
                table: "DishTransaction",
                column: "DiscId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTransaction_TransactionId",
                table: "DishTransaction",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateCategories_TenantId",
                table: "PlateCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMachineSyncStatus_PlateId",
                table: "PlateMachineSyncStatus",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenuMachineSyncStatus_PlateMenuId",
                table: "PlateMenuMachineSyncStatus",
                column: "PlateMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_PlateId",
                table: "PlateMenus",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_ProductId",
                table: "PlateMenus",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_SessionId",
                table: "PlateMenus",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Plates_PlateCategoryId",
                table: "Plates",
                column: "PlateCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Plates_TenantId",
                table: "Plates",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceStrategies_PlateMenus_PlateMenuId",
                table: "PriceStrategies",
                column: "PlateMenuId",
                principalTable: "PlateMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
