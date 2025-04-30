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
    public class Rate
    {
        public Rate()
        {
            Range = new HashSet<RateRange>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long ServiceRevenueId { get; set; }
        public string RevenueCode { get; set; }
        public string RevenueName { get; set; }
        public string AgencyCode { get; set; }
        public long? TypeId { get; set; }
        public long? ServiceId { get; set; }
        public long? ZoneId { get; set; }
        public long? LocationId { get; set; }
        public string Amount { get; set; }
        public string ApprovalComment { get; set; }
        public bool? IsFormulaeRequired { get; set; }
        public bool? IsAmountAutomatic { get; set; }
        public bool? IsDepositRequired { get; set; }
        public long? ServiceMethodId { get; set; }
        public long? InputDefinitionId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public bool? IsApproved { get; set; }
        public string ApprovedOrDissaprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }

        //public bool? IsFormulaeRequired { get; set; }
        //public bool? IsCompareRequired { get; set; }
        //public bool? IsSecondary { get; set; }
        public virtual ServiceMethod ServiceMethod { get; set; }
        public virtual Location Location { get; set; }
        public virtual Service Service { get; set; }
        public virtual ServiceRevenue ServiceRevenue { get; set; }
        public virtual Types Type { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual ICollection<RateRange> Range { get; set; }
    }
}
