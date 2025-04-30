using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class Service
    {
        public Service()
        {
            Rate = new HashSet<Rate>();
            ServiceDetails = new HashSet<ServiceDetails>();
            ServiceRevenue = new HashSet<ServiceRevenue>();
            InputDefinitionMapping = new HashSet<InputDefinitionMapping>();
        }

        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string PaymentPeriod { get; set; }
        public bool? ReminderRequired { get; set; }
        public string AgencyCode { get; set; }
        public long? DeptId { get; set; }
        public string Description { get; set; }
        public string ServiceHeader { get; set; }
        public bool? IsValueInputRequired { get; set; }
        public string ServiceSubHeader { get; set; }
        public bool IsStandardLetterRequired { get; set; }
        public bool IsAdditionalAssessmentRequired { get; set; } = false;
        public bool IsPrimaryAssessment { get; set; } = false;
        public bool IsManualAssessment { get; set; } = false;
        public long? ExternalServiceId { get; set; }
        public bool? IsLiabilityRequired { get; set; }
        public string StandardLetterDescriptions { get; set; }
        public string SignatoryName { get; set; }
        public string SignatoryPosition { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsForAllZones { get; set; }
        public bool? IsServiceApplicableToAll { get; set; }

        public virtual Agency AgencyCodeNavigation { get; set; }
        public virtual Department Dept { get; set; }
        public virtual ICollection<Rate> Rate { get; set; }
        public virtual ICollection<ServiceDetails> ServiceDetails { get; set; }
        public virtual ICollection<ServiceRevenue> ServiceRevenue { get; set; }
        public virtual ICollection<InputDefinitionMapping> InputDefinitionMapping { get; set; }
    }
}
