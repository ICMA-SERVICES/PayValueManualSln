namespace PayValueManualSln.Application.DTOs.MenuSetup
{
    public class UpdateMenuDTO
    {
        public int MenuSetupId { get; set; }
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public string ParentMenuId { get; set; }
        public string MenuUrl { get; set; }
        public string ModuleName { get; set; }
        public bool IsActive { get; set; }
        public bool? IsSubMenu { get; set; }
        public string RoleId { get; set; }
        public string IconClass { get; set; }
        public string RoleName { get; set; }
        public bool RequiresApproval { get; set; }
        public string AppCode { get; set; }
        public bool? IsGeneral { get; set; }
    }
}
