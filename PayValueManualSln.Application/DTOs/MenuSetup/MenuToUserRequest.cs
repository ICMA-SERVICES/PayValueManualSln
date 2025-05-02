using System.Collections.Generic;

namespace PayValueManualSln.Application.DTOs.MenuSetup
{
    public class MenuToUserRequest
    {
        public string UserId { get; set; }
        public List<string> Menus { get; set; }
    }
}
