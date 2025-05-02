using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class InitialAppMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Setting");

            migrationBuilder.EnsureSchema(
                name: "Assessment");

            migrationBuilder.AddColumn<string>(
                name: "AgencyCode1",
                schema: "dbo",
                table: "Services",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                schema: "dbo",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DepartmentId",
                schema: "dbo",
                table: "Services",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Liability",
                schema: "dbo",
                table: "BillInfo",
                type: "decimal(18,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AddColumn<bool>(
                name: "IsRenewed",
                schema: "dbo",
                table: "BillDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalDate",
                schema: "dbo",
                table: "BillDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgencyLogo",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyLogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audit",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "BillAdditionalInfo",
                schema: "Assessment",
                columns: table => new
                {
                    AdditionalInfoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdditionalServiceDetailId = table.Column<int>(type: "int", nullable: false),
                    BillInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_BillAdditionalInfo", x => x.AdditionalInfoId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepositOnConsents",
                schema: "Assessment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRefNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssessmentRefNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InUse = table.Column<bool>(type: "bit", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_DepositOnConsents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Input",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InputName = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_Input", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                schema: "Setting",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.ModuleId);
                });

            migrationBuilder.CreateTable(
                name: "RenewalFrequency",
                schema: "Setting",
                columns: table => new
                {
                    FrequencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrequencyValue = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalFrequency", x => x.FrequencyId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMethodSetups",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceMethodCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MinimumLandSize = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ActualRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceMethodSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zone",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zone", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Type",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Type", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Type_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Setting",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantConfig",
                columns: table => new
                {
                    MerchantConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BgImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateFooter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantWebSite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantPhone1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepositOnConsentRevenue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyPhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandSize = table.Column<int>(type: "int", nullable: true),
                    PaymentForGovernorsConsentOnDeedOfAssignment = table.Column<int>(type: "int", nullable: true),
                    PaymentForLandAllocation = table.Column<int>(type: "int", nullable: true),
                    PaymentForStateLandRevalidationOnTitle1 = table.Column<int>(type: "int", nullable: true),
                    PaymentForStateLandRevalidationOnTitle2 = table.Column<int>(type: "int", nullable: true),
                    PaymentForLandAccommodation = table.Column<int>(type: "int", nullable: true),
                    PaymentForRatification = table.Column<int>(type: "int", nullable: true),
                    StateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppModuleModuleId = table.Column<int>(type: "int", nullable: true),
                    IsManualAssessment = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantConfig", x => x.MerchantConfigId);
                    table.ForeignKey(
                        name: "FK_MerchantConfig_Module_AppModuleModuleId",
                        column: x => x.AppModuleModuleId,
                        principalSchema: "Setting",
                        principalTable: "Module",
                        principalColumn: "ModuleId");
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<long>(type: "bigint", nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_Zone_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zone",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Agency",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    MerchantCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficialAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficialAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficialAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorizedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorizedPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovernorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAutoRenewal = table.Column<bool>(type: "bit", nullable: false),
                    ExternalPaymentCodeRequired = table.Column<bool>(type: "bit", nullable: true),
                    AgencyLogoId = table.Column<int>(type: "int", nullable: false),
                    AuthorizedSignatureId = table.Column<int>(type: "int", nullable: false),
                    MerchantCodeNavigationMerchantConfigId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    TypesId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Agency", x => new { x.Code, x.Id });
                    table.ForeignKey(
                        name: "FK_Agency_AgencyLogo_AgencyLogoId",
                        column: x => x.AgencyLogoId,
                        principalSchema: "Setting",
                        principalTable: "AgencyLogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agency_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Setting",
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agency_MerchantConfig_MerchantCodeNavigationMerchantConfigId",
                        column: x => x.MerchantCodeNavigationMerchantConfigId,
                        principalTable: "MerchantConfig",
                        principalColumn: "MerchantConfigId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agency_Type_TypesId",
                        column: x => x.TypesId,
                        principalTable: "Type",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ValueTemplateForLocation",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypesId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ValueTemplateForLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValueTemplateForLocation_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ValueTemplateForLocation_Type_TypesId",
                        column: x => x.TypesId,
                        principalTable: "Type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgencySignature",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RentRevisionPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencySignature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencySignature_Agency_AgencyCode_Id",
                        columns: x => new { x.AgencyCode, x.Id },
                        principalTable: "Agency",
                        principalColumns: new[] { "Code", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyCodeNavigationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgencyCodeNavigationId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Department_Agency_AgencyCodeNavigationCode_AgencyCodeNavigationId",
                        columns: x => new { x.AgencyCodeNavigationCode, x.AgencyCodeNavigationId },
                        principalTable: "Agency",
                        principalColumns: new[] { "Code", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMethod",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceMethodCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceRevenueId = table.Column<long>(type: "bigint", nullable: true),
                    Formular = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyCodeNavigationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgencyCodeNavigationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceMethod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceMethod_Agency_AgencyCodeNavigationCode_AgencyCodeNavigationId",
                        columns: x => new { x.AgencyCodeNavigationCode, x.AgencyCodeNavigationId },
                        principalTable: "Agency",
                        principalColumns: new[] { "Code", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReminderRequired = table.Column<bool>(type: "bit", nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceHeader = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsValueInputRequired = table.Column<bool>(type: "bit", nullable: true),
                    ServiceSubHeader = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStandardLetterRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsAdditionalAssessmentRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryAssessment = table.Column<bool>(type: "bit", nullable: false),
                    IsManualAssessment = table.Column<bool>(type: "bit", nullable: false),
                    ExternalServiceId = table.Column<long>(type: "bigint", nullable: true),
                    IsLiabilityRequired = table.Column<bool>(type: "bit", nullable: true),
                    StandardLetterDescriptions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignatoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignatoryPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    IsForAllZones = table.Column<bool>(type: "bit", nullable: true),
                    IsServiceApplicableToAll = table.Column<bool>(type: "bit", nullable: true),
                    AgencyCodeNavigationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgencyCodeNavigationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Service_Agency_AgencyCodeNavigationCode_AgencyCodeNavigationId",
                        columns: x => new { x.AgencyCodeNavigationCode, x.AgencyCodeNavigationId },
                        principalTable: "Agency",
                        principalColumns: new[] { "Code", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Service_Department_DeptId",
                        column: x => x.DeptId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserDepartment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDepartment_Department_DeptId",
                        column: x => x.DeptId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputDefinitionMapping",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InputId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServicesId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_InputDefinitionMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputDefinitionMapping_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InputDefinitionMapping_Services_ServicesId",
                        column: x => x.ServicesId,
                        principalSchema: "dbo",
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    DetailName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalInfoId = table.Column<long>(type: "bigint", nullable: false),
                    BillAdditionalInfoAdditionalInfoId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ServiceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceDetails_BillAdditionalInfo_BillAdditionalInfoAdditionalInfoId",
                        column: x => x.BillAdditionalInfoAdditionalInfoId,
                        principalSchema: "Assessment",
                        principalTable: "BillAdditionalInfo",
                        principalColumn: "AdditionalInfoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceDetails_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRevenue",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevenueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormulaeValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdditionalInfoRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsPaymentCodeEnabled = table.Column<bool>(type: "bit", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRenewable = table.Column<bool>(type: "bit", nullable: false),
                    AllowAutomaticTrigger = table.Column<bool>(type: "bit", nullable: false),
                    AutomaticApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsRenewableByDate = table.Column<bool>(type: "bit", nullable: true),
                    RenewalFrequencyId = table.Column<int>(type: "int", nullable: false),
                    ServiceMethodId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRevenue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRevenue_RenewalFrequency_RenewalFrequencyId",
                        column: x => x.RenewalFrequencyId,
                        principalSchema: "Setting",
                        principalTable: "RenewalFrequency",
                        principalColumn: "FrequencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRevenue_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRevenue_ServiceMethod_ServiceMethodId",
                        column: x => x.ServiceMethodId,
                        principalTable: "ServiceMethod",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRevenueId = table.Column<long>(type: "bigint", nullable: false),
                    RevenueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: true),
                    ServiceId = table.Column<long>(type: "bigint", nullable: true),
                    ZoneId = table.Column<long>(type: "bigint", nullable: true),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFormulaeRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsAmountAutomatic = table.Column<bool>(type: "bit", nullable: true),
                    IsDepositRequired = table.Column<bool>(type: "bit", nullable: true),
                    ServiceMethodId = table.Column<long>(type: "bigint", nullable: true),
                    InputDefinitionId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    ApprovedOrDissaprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rate_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rate_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rate_ServiceMethod_ServiceMethodId",
                        column: x => x.ServiceMethodId,
                        principalTable: "ServiceMethod",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rate_ServiceRevenue_ServiceRevenueId",
                        column: x => x.ServiceRevenueId,
                        principalSchema: "Setting",
                        principalTable: "ServiceRevenue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rate_Type_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Type",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rate_Zone_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zone",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Range",
                schema: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RateId = table.Column<long>(type: "bigint", nullable: true),
                    MinimumValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    MaximumValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    RateValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMultiplyRange = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_Range", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Range_Rate_RateId",
                        column: x => x.RateId,
                        principalTable: "Rate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_AgencyCode1_AgencyId",
                schema: "dbo",
                table: "Services",
                columns: new[] { "AgencyCode1", "AgencyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_DepartmentId",
                schema: "dbo",
                table: "Services",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Agency_AgencyLogoId",
                table: "Agency",
                column: "AgencyLogoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agency_CategoryId",
                table: "Agency",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Agency_MerchantCodeNavigationMerchantConfigId",
                table: "Agency",
                column: "MerchantCodeNavigationMerchantConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Agency_TypesId",
                table: "Agency",
                column: "TypesId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencySignature_AgencyCode_Id",
                schema: "Setting",
                table: "AgencySignature",
                columns: new[] { "AgencyCode", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_AgencyCodeNavigationCode_AgencyCodeNavigationId",
                table: "Department",
                columns: new[] { "AgencyCodeNavigationCode", "AgencyCodeNavigationId" });

            migrationBuilder.CreateIndex(
                name: "IX_InputDefinitionMapping_ServiceId",
                schema: "Setting",
                table: "InputDefinitionMapping",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InputDefinitionMapping_ServicesId",
                schema: "Setting",
                table: "InputDefinitionMapping",
                column: "ServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ZoneId",
                table: "Location",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantConfig_AppModuleModuleId",
                table: "MerchantConfig",
                column: "AppModuleModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Range_RateId",
                schema: "Setting",
                table: "Range",
                column: "RateId");

            migrationBuilder.CreateIndex(
                name: "IX_Rate_LocationId",
                table: "Rate",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Rate_ServiceId",
                table: "Rate",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Rate_ServiceMethodId",
                table: "Rate",
                column: "ServiceMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Rate_ServiceRevenueId",
                table: "Rate",
                column: "ServiceRevenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Rate_TypeId",
                table: "Rate",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rate_ZoneId",
                table: "Rate",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_AgencyCodeNavigationCode_AgencyCodeNavigationId",
                table: "Service",
                columns: new[] { "AgencyCodeNavigationCode", "AgencyCodeNavigationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Service_DeptId",
                table: "Service",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDetails_BillAdditionalInfoAdditionalInfoId",
                table: "ServiceDetails",
                column: "BillAdditionalInfoAdditionalInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDetails_ServiceId",
                table: "ServiceDetails",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMethod_AgencyCodeNavigationCode_AgencyCodeNavigationId",
                table: "ServiceMethod",
                columns: new[] { "AgencyCodeNavigationCode", "AgencyCodeNavigationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRevenue_RenewalFrequencyId",
                schema: "Setting",
                table: "ServiceRevenue",
                column: "RenewalFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRevenue_ServiceId",
                schema: "Setting",
                table: "ServiceRevenue",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRevenue_ServiceMethodId",
                schema: "Setting",
                table: "ServiceRevenue",
                column: "ServiceMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Type_CategoryId",
                table: "Type",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_DeptId",
                table: "UserDepartment",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_ValueTemplateForLocation_LocationId",
                schema: "Setting",
                table: "ValueTemplateForLocation",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ValueTemplateForLocation_TypesId",
                schema: "Setting",
                table: "ValueTemplateForLocation",
                column: "TypesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Agency_AgencyCode1_AgencyId",
                schema: "dbo",
                table: "Services",
                columns: new[] { "AgencyCode1", "AgencyId" },
                principalTable: "Agency",
                principalColumns: new[] { "Code", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Department_DepartmentId",
                schema: "dbo",
                table: "Services",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Agency_AgencyCode1_AgencyId",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Department_DepartmentId",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropTable(
                name: "AgencySignature",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "Audit");

            migrationBuilder.DropTable(
                name: "DepositOnConsents",
                schema: "Assessment");

            migrationBuilder.DropTable(
                name: "Input",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "InputDefinitionMapping",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "Range",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "ServiceDetails");

            migrationBuilder.DropTable(
                name: "ServiceMethodSetups",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "UserDepartment");

            migrationBuilder.DropTable(
                name: "ValueTemplateForLocation",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "Rate");

            migrationBuilder.DropTable(
                name: "BillAdditionalInfo",
                schema: "Assessment");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "ServiceRevenue",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "Zone");

            migrationBuilder.DropTable(
                name: "RenewalFrequency",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "ServiceMethod");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Agency");

            migrationBuilder.DropTable(
                name: "AgencyLogo",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "MerchantConfig");

            migrationBuilder.DropTable(
                name: "Type");

            migrationBuilder.DropTable(
                name: "Module",
                schema: "Setting");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Setting");

            migrationBuilder.DropIndex(
                name: "IX_Services_AgencyCode1_AgencyId",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_DepartmentId",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AgencyCode1",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "dbo",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsRenewed",
                schema: "dbo",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "RenewalDate",
                schema: "dbo",
                table: "BillDetails");

            migrationBuilder.AlterColumn<decimal>(
                name: "Liability",
                schema: "dbo",
                table: "BillInfo",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldNullable: true);
        }
    }
}
