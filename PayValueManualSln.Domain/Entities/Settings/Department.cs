using PayValueManualSln.Domain.Common;
using PayValueManualSln.Domain.Entities.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class Department : BaseEntity
    {
        public Department()
        {
            Services = new HashSet<Services>();
            UserDepartment = new HashSet<UserDepartment>();
        }

        [Key]
        public long Id { get; set; }
        public string AgencyCode { get; set; }
        public string Name { get; set; }

        public virtual Agency AgencyCodeNavigation { get; set; }
        public virtual ICollection<Services> Services { get; set; }
        public virtual ICollection<UserDepartment> UserDepartment { get; set; }
    }
}
