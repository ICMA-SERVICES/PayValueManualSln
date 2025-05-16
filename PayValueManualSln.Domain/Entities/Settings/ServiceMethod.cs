using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class ServiceMethod
    {
        public ServiceMethod()
        {
            Rate = new HashSet<Rate>();
            ServiceRevenue = new HashSet<ServiceRevenue>();
        }

        [Key]
        public long Id { get; set; }
        public string AgencyCode { get; set; }
        public string ServiceMethodCode { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public long? ServiceRevenueId { get; set; }
        public string Formular { get; set; }    

        public virtual Agency AgencyCodeNavigation { get; set; }
        public virtual ICollection<ServiceRevenue> ServiceRevenue { get; set; }
        public virtual ICollection<Rate> Rate { get; set; }
    }
}
