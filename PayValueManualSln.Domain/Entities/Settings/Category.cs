using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("Categories", Schema = "Setting")]
    public class Category : BaseEntity
    {
        public Category()
        {
            Agency = new HashSet<Agency>();
            Types = new HashSet<Types>();
        }
        [Key]
        public long Id { get; set; }
        public string CategoryName { get; set; }
        public long TypeId { get; set; }
        public string AgencyCode { get; set; }
        public bool? IsActive { get; set; } = true;

        public virtual ICollection<Types> Types { get; set; }
        public virtual ICollection<Agency> Agency { get; set; }

    }
}
