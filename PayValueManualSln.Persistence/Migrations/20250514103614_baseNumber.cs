using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class baseNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreviousBaseNumber",
                schema: "dbo",
                table: "BillDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreviousBaseNumber",
                schema: "dbo",
                table: "BillDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
