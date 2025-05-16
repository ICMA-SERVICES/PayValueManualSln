using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
    }
}
