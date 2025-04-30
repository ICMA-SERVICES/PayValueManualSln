using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class Zone
    {
        public Zone()
        {
            Location = new HashSet<Location>();
            Rate = new HashSet<Rate>();
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

        public virtual ICollection<Location> Location { get; set; }
        public virtual ICollection<Rate> Rate { get; set; }
    }
}
