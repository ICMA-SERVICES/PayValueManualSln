using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{
    [Table("DepositOnConsents", Schema = "Assessment")]
    public class DepositOnConsent : BaseEntity
    {
        public long Id { get; set; }
        public string PaymentRefNumber { get; set; }
        public string PayerId { get; set; }
        public string AssessmentRefNumber { get; set; }
        public bool? InUse { get; set; }
        public bool? IsUsed { get; set; }
    }
}
