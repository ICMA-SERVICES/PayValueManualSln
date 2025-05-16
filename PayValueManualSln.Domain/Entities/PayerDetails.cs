using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{

    [Table("PayerDetails")]
    public class PayerDetails
    {
        public string? taxPayerReferenceNumber { get; set; }
        [Key]
        public string payerUtin { get; set; }
        public string? surname { get; set; }
        public string firstName { get; set; }
        public string otherName { get; set; }
        public DateTime dateofBirth { get; set; }
        public string email { get; set; }
        public string? regTypeCode { get; set; }
        public DateTime dateCreated { get; set; }
        public string? address { get; set; }
        public string? address1 { get; set; }
        public string? address2 { get; set; }
        public string? photograph { get; set; }
        public string? contentType { get; set; }
        public string? fileName { get; set; }
        public string? signature { get; set; }
        public string? courtesyTitle { get; set; }
        public string? genderType { get; set; }
        public string? phoneNo { get; set; }
        public string? phoneNo1 { get; set; }
        public string? phoneNo2 { get; set; }
        public string? phoneNo3 { get; set; }
        public string jtbTin { get; set; }
        public string? employeeName { get; set; }
        public string? taxAgentReferenceNumber { get; set; }
        public string? revenueOfficeName { get; set; }
        public string? staffNumber { get; set; }
        public int? classOfEmployeeId { get; set; }
        public string? classOfEmployeeName { get; set; }
        public string payerType { get; set; }
        public string? payerCategory { get; set; }
        public string payerName { get; set; }
        public string? fullPayerName { get; set; }
        public string utin { get; set; }
        public string? contactPersonEmail { get; set; }
        public string? telephoneNumber { get; set; }
        public string? merchantCode { get; set; }
        public int lgaId { get; set; }
        public string? lgaName { get; set; }
        public string cacNumber { get; set; } 
        public string revenueOfficeID { get; set; }
        public string contactName { get; set; }
        public bool isPramary { get; set; }
        public bool isParent { get; set; }
        public int? businessTypeId { get; set; }
        public int? townID { get; set; }
        public bool? IsApproved { get; set; }
        public string ApprovalComment { get; set; }
        public string ChangeRequesterId { get; set; }

        public DateTime? RequestedOn { get; set; }
        public DateTime? ActedUponOn { get; set; }
    }
}
