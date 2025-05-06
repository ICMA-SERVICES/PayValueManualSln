using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class AssessmentApprovalRequest
    {
        public long AssessmentId { get; set; }
        public bool IsApproved { get; set; }
        public string Comment { get; set; }
    }
}
