using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayValueManualSln.Persistence.Migrations
{
    public partial class payer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayerDetails",
                columns: table => new
                {
                    payerUtin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    taxPayerReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    otherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dateofBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    regTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    photograph = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    signature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    courtesyTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    genderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNo1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNo2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNo3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    jtbTin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    employeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    taxAgentReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    revenueOfficeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    staffNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    classOfEmployeeId = table.Column<int>(type: "int", nullable: false),
                    classOfEmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payerCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fullPayerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    utin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contactPersonEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    merchantCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lgaId = table.Column<int>(type: "int", nullable: false),
                    lgaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cacNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    revenueOfficeID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isPramary = table.Column<bool>(type: "bit", nullable: false),
                    isParent = table.Column<bool>(type: "bit", nullable: false),
                    businessTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    townID = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    ApprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeRequesterId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActedUponOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayerDetails", x => x.payerUtin);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayerDetails");
        }
    }
}
