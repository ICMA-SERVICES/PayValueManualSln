using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class UpdatePayerRequest
    {
        //AGENT PAYER
        public string companyName { get; set; }
        public string payerUtin { get; set; }
        public string companyAddress { get; set; }
        public string cacRegNumber { get; set; }
        public string contactName { get; set; }
        public string contactPhoneNo { get; set; }
        public string designation { get; set; }
        public string salution { get; set; }
        public string contactEmail { get; set; }
        public string companyEmail { get; set; }
        public string revenueOfficeID { get; set; }
        public int? businessTypeId { get; set; }
        public int? businessOwnshipID { get; set; }
        public int? operationalStateId { get; set; }
        public int? operationalLgaId { get; set; }
        public string nin { get; set; }
        public string jtbtin { get; set; }


        //INDIVIDUAL PAYER
        public string courtesyTitle { get; set; }
        public string surname { get; set; }
        public string firstName { get; set; }
        public string otherName { get; set; }
        public string utin { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        //public int revenueOfficeID { get; set; }

        public string payerType { get; set; }
        public string payerCategory { get; set; }
        public bool? IsApproved { get; set; } = null;
        public string ApprovalComment { get; set; }
        public int townId { get; set; }
        public int lgaId { get; set; }
    }
}
