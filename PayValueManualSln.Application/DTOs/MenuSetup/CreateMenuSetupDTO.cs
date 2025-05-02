namespace PayValueManualSln.Application.DTOs.MenuSetup
{
    public class CreateMenuSetupDTO
    {
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public string ParentMenuId { get; set; }
        public string MenuUrl { get; set; }
        public bool IsActive { get; set; }
        public bool? IsSubMenu { get; set; }
        public string IconClass { get; set; }
        public bool? IsGeneral { get; set; }
        public string RoleName { get; set; }
        public bool? IsHeadOfUnitPage { get; set; } = false;
        public bool? IsHeadOfDepartmentPage { get; set; } = false;
        public bool RequiresApproval { get; set; }
    }
}
