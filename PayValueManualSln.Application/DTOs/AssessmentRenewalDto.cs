using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class AssessmentRenewalDto
    {
        public bool IsRenewable { get; set; } = false;
        public bool? AllowAutomaticTrigger { get; set; } = false;
        public bool AutomaticApproval { get; set; } = false;
        public bool? IsRenewableByDate { get; set; } = false;
        public int RenewalFrequencyId { get; set; }
        public int? FrequencyValue { get; set; }
        public long ServiceRevenueId { get; set; }
        public DateTime AssessmentCreatedDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public int BillDetailId { get; set; }
    }
}
