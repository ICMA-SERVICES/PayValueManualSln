using PayValueManualSln.Application.Dtos.Shared;
using PayValueManualSln.Application.DTOs.MenuSetup;
using PayValueManualSln.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PayValueManualSln.Application.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<MenuSetupDTO>> GetMenu(string RoleID);
        Task<List<MenuSetupDTO>> GetMenuForAssigningToUser(string RoleID);
        Task<List<MenuSetupDTO>> GetPagesAssignedToUser(string UserID, string userPageSecret, string userroleid);
        // Task<string> GetStoreName();
        RepositoryResponse CreateMenuSetup(CreateMenuSetupDTO createMenuDTO);
        Task<int> AssignMenusToUser(MenuToUserRequest model);
        Task<int> DeleteMenuAssignedToUser(MenuToUserRequest model);
        Task<List<MenuSetupDTO>> GetMenusWithApprovalSettingAsTrue();
        Task<MerchantConfigs> GetMerchantSetup(string url);
        Task<List<MenuSetupDTO>> GetAllMenu();
        Task<Response<string>> UpdateMenu(UpdateMenuDTO model);
        Task<Response<string>> DeleteMenu(int MenuSetupId);
    }
}
