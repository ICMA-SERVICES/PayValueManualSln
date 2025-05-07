using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations.IcmaCollection
{
    public partial class IcmaCollection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Collection");

            migrationBuilder.CreateTable(
                name: "CollectionAgency",
                schema: "Collection",
                columns: table => new
                {
                    AgencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBIR = table.Column<bool>(type: "bit", nullable: true),
                    isInternal = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionAgency", x => x.AgencyId);
                });

            migrationBuilder.CreateTable(
                name: "CollectionReport",
                schema: "Collection",
                columns: table => new
                {
                    PaymentRefNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ColProviderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChannelCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    AmountUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepositSlipNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsDateExtended = table.Column<bool>(type: "bit", nullable: true),
                    TelephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PaymentValueStatusCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ValueStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ValueDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReceiptNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IcmaReceipt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IcmaReceiptDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RevenueId = table.Column<long>(type: "bigint", nullable: false),
                    RevenueCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RevenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    AgencyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    BranchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChequeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChequeBankCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChequeBankName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PostedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    ReversedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReversalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssessmentNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ControlNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsModified = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    IsUsed = table.Column<bool>(type: "bit", nullable: true),
                    IsSplitted = table.Column<bool>(type: "bit", nullable: true),
                    PayerUtin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalisedBy = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    NormalisedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    PlatformCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Balance = table.Column<decimal>(type: "money", nullable: true),
                    StateCode = table.Column<int>(type: "int", nullable: true),
                    IsPaymentPeriodSplitted = table.Column<bool>(type: "bit", nullable: false),
                    IsPushedToPlatformOwner = table.Column<bool>(type: "bit", nullable: false),
                    FinalUTIN = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    isRepositoryUpdate = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsedByPlatform = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionReport", x => x.PaymentRefNumber);
                });

            migrationBuilder.CreateTable(
                name: "CollectionRevenue",
                schema: "Collection",
                columns: table => new
                {
                    RevenueId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevenueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    IsBIR = table.Column<bool>(type: "bit", nullable: true),
                    isInternal = table.Column<bool>(type: "bit", nullable: true),
                    MinimunAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillValidationRequired = table.Column<bool>(type: "bit", nullable: true),
                    PayerValidationRequired = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionRevenue", x => x.RevenueId);
                });

            migrationBuilder.CreateIndex(
                name: "idx_Clustered",
                schema: "Collection",
                table: "CollectionReport",
                columns: new[] { "ColProviderCode", "ChannelCode", "DepositSlipNumber", "PaymentMethodCode", "MerchantCode", "PaymentDate", "Amount" });

            migrationBuilder.CreateIndex(
                name: "idx_NonClustered_dobyLastThreeYears",
                schema: "Collection",
                table: "CollectionReport",
                columns: new[] { "ColProviderCode", "ChannelCode", "DepositSlipNumber", "PaymentMethodCode", "AgencyId", "PaymentDate", "Amount", "ReversalId" });

            migrationBuilder.CreateIndex(
                name: "idx_PaymentDate",
                schema: "Collection",
                table: "CollectionReport",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "idx_PaymentRefNumber",
                schema: "Collection",
                table: "CollectionReport",
                column: "PaymentRefNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionAgency",
                schema: "Collection");

            migrationBuilder.DropTable(
                name: "CollectionReport",
                schema: "Collection");

            migrationBuilder.DropTable(
                name: "CollectionRevenue",
                schema: "Collection");
        }
    }
}
