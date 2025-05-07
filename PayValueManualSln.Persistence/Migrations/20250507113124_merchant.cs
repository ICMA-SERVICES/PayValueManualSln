using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class merchant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agency_MerchantConfig_MerchantCodeNavigationMerchantConfigId",
                table: "Agency");

            migrationBuilder.DropIndex(
                name: "IX_Agency_MerchantCodeNavigationMerchantConfigId",
                table: "Agency");

            migrationBuilder.DropColumn(
                name: "MerchantCodeNavigationMerchantConfigId",
                table: "Agency");

            migrationBuilder.AddColumn<int>(
                name: "MerchantConfigId",
                table: "Agency",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agency_MerchantConfigId",
                table: "Agency",
                column: "MerchantConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agency_MerchantConfig_MerchantConfigId",
                table: "Agency",
                column: "MerchantConfigId",
                principalTable: "MerchantConfig",
                principalColumn: "MerchantConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agency_MerchantConfig_MerchantConfigId",
                table: "Agency");

            migrationBuilder.DropIndex(
                name: "IX_Agency_MerchantConfigId",
                table: "Agency");

            migrationBuilder.DropColumn(
                name: "MerchantConfigId",
                table: "Agency");

            migrationBuilder.AddColumn<int>(
                name: "MerchantCodeNavigationMerchantConfigId",
                table: "Agency",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Agency_MerchantCodeNavigationMerchantConfigId",
                table: "Agency",
                column: "MerchantCodeNavigationMerchantConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agency_MerchantConfig_MerchantCodeNavigationMerchantConfigId",
                table: "Agency",
                column: "MerchantCodeNavigationMerchantConfigId",
                principalTable: "MerchantConfig",
                principalColumn: "MerchantConfigId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
