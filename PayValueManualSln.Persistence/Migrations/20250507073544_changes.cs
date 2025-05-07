using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "BillInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "dbo",
                table: "BillInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "dbo",
                table: "BillInfo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "BillInfo",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRenewed",
                schema: "dbo",
                table: "BillInfo",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                schema: "dbo",
                table: "BillInfo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "dbo",
                table: "BillInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousBaseNumber",
                schema: "dbo",
                table: "BillDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousItemPaymentCode",
                schema: "dbo",
                table: "BillDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MapUserApproval",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitiatorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidatorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorizerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapUserApproval", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuSetup",
                columns: table => new
                {
                    MenuSetupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentMenuId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenuUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSubMenu = table.Column<bool>(type: "bit", nullable: true),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: true),
                    IsGeneral = table.Column<bool>(type: "bit", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuSetup", x => x.MenuSetupId);
                });

            migrationBuilder.CreateTable(
                name: "ModuleApprovalConfig",
                columns: table => new
                {
                    ModuleApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsApprovalForGeneralValidators = table.Column<bool>(type: "bit", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoOfCheckersRequired = table.Column<int>(type: "int", nullable: true),
                    NoOfAuthorizersRequired = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    IsApprovalSequence = table.Column<bool>(type: "bit", nullable: true),
                    ModuleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleApprovalConfig", x => x.ModuleApprovalId);
                });

            migrationBuilder.CreateTable(
                name: "UsersRolePermission",
                columns: table => new
                {
                    UsersRolePermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuSetupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSubMenu = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersRolePermission", x => x.UsersRolePermissionId);
                    table.ForeignKey(
                        name: "FK_UsersRolePermission_MenuSetup_MenuSetupId",
                        column: x => x.MenuSetupId,
                        principalTable: "MenuSetup",
                        principalColumn: "MenuSetupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersRolePermission_MenuSetupId",
                table: "UsersRolePermission",
                column: "MenuSetupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapUserApproval",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "ModuleApprovalConfig");

            migrationBuilder.DropTable(
                name: "UsersRolePermission");

            migrationBuilder.DropTable(
                name: "MenuSetup");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "IsRenewed",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "dbo",
                table: "BillInfo");

            migrationBuilder.DropColumn(
                name: "PreviousBaseNumber",
                schema: "dbo",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "PreviousItemPaymentCode",
                schema: "dbo",
                table: "BillDetails");
        }
    }
}
