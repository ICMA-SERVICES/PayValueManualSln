using PayValueManualSln.Domain.Entities.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("ServiceRevenue", Schema = "Setting")]
    public partial class ServiceRevenue
    {
        public ServiceRevenue()
        {
            Rate = new HashSet<Rate>();
        }

        [Key]
        public long Id { get; set; }
        public string RevenueCode { get; set; }
        public string RevenueName { get; set; }
        public long ServiceId { get; set; }
        public string PaymentItemName { get; set; }
        public string FormulaeValue { get; set; }
        public bool? IsAdditionalInfoRequired { get; set; }
        public bool? IsPaymentCodeEnabled { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }

        public bool IsRenewable { get; set; } = false;
        public bool AllowAutomaticTrigger { get; set; } = false;
        public bool AutomaticApproval { get; set; } = false;
        public bool? IsRenewableByDate { get; set; } = false;
        public int RenewalFrequencyId { get; set; }

        public virtual Services Service { get; set; }
        public RenewalFrequency RenewalFrequency { get; set; }
        public virtual ICollection<Rate> Rate { get; set; }
    }
}
