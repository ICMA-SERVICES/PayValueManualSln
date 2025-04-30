using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class Types
    {
        public Types()
        {
            Rate = new HashSet<Rate>();
            ValueTemplateForLocation = new HashSet<ValueTemplateForLocation>();
        }

        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string AgencyCode { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }

        public virtual ICollection<Rate> Rate { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Agency> Agency { get; set; }
        public virtual ICollection<ValueTemplateForLocation> ValueTemplateForLocation { get; set; }
    }
}
