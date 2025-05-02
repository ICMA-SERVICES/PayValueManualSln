using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class MenuSetup
    {
        public MenuSetup()
        {
            UsersRolePermission = new HashSet<UsersRolePermission>();
        }
        [Key]
        public int MenuSetupId { get; set; }
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public string ParentMenuId { get; set; }
        public string MenuUrl { get; set; }
        public bool IsActive { get; set; }
        public bool? IsSubMenu { get; set; }
        public string RoleId { get; set; }
        public string IconClass { get; set; }
        public string RoleName { get; set; }
        public bool? RequiresApproval { get; set; }
        public bool? IsGeneral { get; set; }
        public DateTime? DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public string ModuleName { get; set; }

        public virtual ICollection<UsersRolePermission> UsersRolePermission { get; set; }
    }
}
