using System;

namespace PayValueManualSln.Application.DTOs.MenuSetup
{
    public class PermissionDTO
    {
        public int UsersRolePermissionId { get; set; }
        public int MenuSetupId { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public bool? IsSubMenu { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
