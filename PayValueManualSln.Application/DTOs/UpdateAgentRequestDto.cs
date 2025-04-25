using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class UpdateAgentRequestDto
    {
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
        public int businessTypeId { get; set; }
        public int businessOwnshipID { get; set; }
        public int operationalStateId { get; set; }
        public int operationalLgaId { get; set; }
        public string nin { get; set; }
        public string jtbtin { get; set; }
        public bool isParent { get; set; }
        public int townId { get; set; }
    }
}
