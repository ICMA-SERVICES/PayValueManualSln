using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class merchantConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "AgencyMerchantConfig",
                columns: table => new
                {
                    MerchantConfigId = table.Column<int>(type: "int", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyMerchantConfig", x => new { x.MerchantConfigId, x.AgencyCode, x.AgencyId });
                    table.ForeignKey(
                        name: "FK_AgencyMerchantConfig_Agency_AgencyCode_AgencyId",
                        columns: x => new { x.AgencyCode, x.AgencyId },
                        principalTable: "Agency",
                        principalColumns: new[] { "Code", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgencyMerchantConfig_MerchantConfig_MerchantConfigId",
                        column: x => x.MerchantConfigId,
                        principalTable: "MerchantConfig",
                        principalColumn: "MerchantConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyMerchantConfig_AgencyCode_AgencyId",
                table: "AgencyMerchantConfig",
                columns: new[] { "AgencyCode", "AgencyId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyMerchantConfig");

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
    }
}
