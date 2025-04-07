using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Assessment",
                schema: "dbo",
                columns: table => new
                {
                    AssessmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentRefNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemPaymentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemTransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Arrears = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TotalAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevenueCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RevenueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Platformcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AmountPaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssessmentBalance = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    PartPaymentAllow = table.Column<bool>(type: "bit", nullable: true),
                    AsExpiryDate = table.Column<bool>(type: "bit", nullable: true),
                    IsExpired = table.Column<bool>(type: "bit", nullable: true),
                    ParentID = table.Column<long>(type: "bigint", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: true),
                    Reversedby = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DateReversed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgentUtin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreviousYearAssessmentRefNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssessmentCreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssessmentCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssessmentApprovedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssessmentDateApproved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PushtoXpress = table.Column<bool>(type: "bit", nullable: true),
                    DatePushtoXpress = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPushedToPayersLedger = table.Column<bool>(type: "bit", nullable: false),
                    PaymentPeriod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment", x => x.AssessmentID);
                });

            migrationBuilder.CreateTable(
                name: "BillInfo",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: true),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerUtin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAssessed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Liability = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    TotalBillAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountPaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdditionalAssessmentRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsPrimaryAssessment = table.Column<bool>(type: "bit", nullable: true),
                    BillPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DissaprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MergerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    NoOfApprovalCount = table.Column<int>(type: "int", nullable: true),
                    NoOfApprovedCount = table.Column<int>(type: "int", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: true),
                    ReversedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReversedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsUpdated = table.Column<bool>(type: "bit", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignatureId = table.Column<int>(type: "int", nullable: true),
                    IsMailSent = table.Column<bool>(type: "bit", nullable: true),
                    IsMailSendingRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsRebated = table.Column<bool>(type: "bit", nullable: true),
                    IsBillWithdrawn = table.Column<bool>(type: "bit", nullable: false),
                    IsSentToAssessmentRepository = table.Column<bool>(type: "bit", nullable: true),
                    IsExpired = table.Column<bool>(type: "bit", nullable: true),
                    ReasonForExpiration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillWithdrawnOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    AdditionalInfoId = table.Column<long>(type: "bigint", nullable: false),
                    BaseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandSize = table.Column<double>(type: "float", nullable: true),
                    Pages = table.Column<double>(type: "float", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(350)", unicode: false, maxLength: 350, nullable: false),
                    PaymentPeriod = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ReminderRequired = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    AgencyCode = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ServiceHeader = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    IsValueInputRequired = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))"),
                    ServiceSubHeader = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    IsStandardLetterRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsAdditionalAssessmentRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryAssessment = table.Column<bool>(type: "bit", nullable: false),
                    IsManualAssessment = table.Column<bool>(type: "bit", nullable: false),
                    ExternalServiceId = table.Column<long>(type: "bigint", nullable: true),
                    IsLiabilityRequired = table.Column<bool>(type: "bit", nullable: true),
                    StandardLetterDescriptions = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    SignatoryName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    SignatoryPosition = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))"),
                    IsForAllZones = table.Column<bool>(type: "bit", nullable: true),
                    IsServiceApplicableToAll = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillDetails",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillInfoGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillInfoId = table.Column<long>(type: "bigint", nullable: true),
                    RateId = table.Column<long>(type: "bigint", nullable: true),
                    ServiceId = table.Column<long>(type: "bigint", nullable: true),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseNumberItemRefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZoneId = table.Column<long>(type: "bigint", nullable: true),
                    ZoneName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDepositRequired = table.Column<bool>(type: "bit", nullable: true),
                    ItemPaymentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentReferenceNum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceRevenueId = table.Column<long>(type: "bigint", nullable: true),
                    BillAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Liability = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsRebated = table.Column<bool>(type: "bit", nullable: true),
                    RebatePercentage = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    RebateAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    RebateRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShowRebateAmount = table.Column<bool>(type: "bit", nullable: true),
                    TotalBillAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillAmountPaid = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    BillBalance = table.Column<decimal>(type: "decimal(18,6)", nullable: true, computedColumnSql: "[TotalBillAmount] - [BillAmountPaid]"),
                    RevenueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartPaymentAllow = table.Column<bool>(type: "bit", nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateById = table.Column<int>(type: "int", nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillDetails_BillInfo_BillInfoId",
                        column: x => x.BillInfoId,
                        principalSchema: "dbo",
                        principalTable: "BillInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Revenue",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevenueCode = table.Column<string>(type: "varchar(350)", unicode: false, maxLength: 350, nullable: false),
                    RevenueName = table.Column<string>(type: "varchar(350)", unicode: false, maxLength: 350, nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentItemName = table.Column<string>(type: "varchar(350)", unicode: false, maxLength: 350, nullable: false),
                    FormulaeValue = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsAdditionalInfoRequired = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))"),
                    IsPaymentCodeEnabled = table.Column<bool>(type: "bit", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revenue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRevenue_Services",
                        column: x => x.ServiceId,
                        principalSchema: "dbo",
                        principalTable: "Services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_BillInfoId",
                schema: "dbo",
                table: "BillDetails",
                column: "BillInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Revenue_ServiceId",
                schema: "dbo",
                table: "Revenue",
                column: "ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assessment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BillDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Revenue",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BillInfo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Services",
                schema: "dbo");
        }
    }
}
