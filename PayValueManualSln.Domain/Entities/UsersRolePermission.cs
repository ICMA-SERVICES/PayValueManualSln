using PayValueManualSln.Domain.Entities.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{
    public partial class UsersRolePermission
    {
        [Key]
        public int UsersRolePermissionId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public Guid Guid { get; set; }
        public int MenuSetupId { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public bool? IsSubMenu { get; set; }

        public virtual MenuSetup MenuSetup { get; set; }
    }
}
