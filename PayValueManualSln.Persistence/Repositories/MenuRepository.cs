using Dapper;
using Microsoft.Extensions.Logging;
using PayValueManualSln.Application.DTOs.MenuSetup;
using PayValueManualSln.Application.Dtos.Shared;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Shared.DapperServices;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayValueManualSln.Application;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Application.Wrappers;

namespace PayValueManualSln.Persistence.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly ILogger<MenuRepository> _logger;
        private readonly IAuthenticatedUserService _authenticatedUser;
        //private string constring;
        //IOptions<ConnectionStrings> myconnectionString;
        private readonly IDapper _dapper;
        private readonly ApplicationDbContext _context;
        private readonly IAuditRepository _auditRepository;

        public MenuRepository(ILogger<MenuRepository> logger,
                               IAuthenticatedUserService authenticatedUser,
                               //IOptions<ConnectionStrings> connectionString,
                               IDapper dapper,
                               ApplicationDbContext context,
                               IAuditRepository auditRepository)
        {
            _logger = logger;
            _authenticatedUser = authenticatedUser;
            //myconnectionString = connectionString;
            _dapper = dapper;
            _context = context;
            _auditRepository = auditRepository;
            //constring = myconnectionString.Value.DefaultConnection;
        }
        public RepositoryResponse CreateMenuSetup(CreateMenuSetupDTO createMenuDTO)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Status", Status.INSERT);
                param.Add("MenuId", createMenuDTO.MenuId);
                param.Add("MenuUrl", createMenuDTO.MenuUrl);
                param.Add("RoleName", createMenuDTO.RoleName);
                param.Add("MenuName", createMenuDTO.MenuName);
                param.Add("IconClass", createMenuDTO.IconClass);
                param.Add("ParentMenuId", createMenuDTO.ParentMenuId);
                param.Add("IconClass", createMenuDTO.IconClass);
                param.Add("CreatedBy", _authenticatedUser.UserId);
                param.Add("RequiresApproval", createMenuDTO.RequiresApproval);
                param.Add("IsGeneral", createMenuDTO.IsGeneral);

                var response = _dapper.Insert<int>(ApplicationConstants.Sp_MenuSetup, param, CommandType.StoredProcedure);
                if (response < 0)
                    return ApplicationConstants.RepositoryExists();
                else if (response == 0)
                    return ApplicationConstants.RepositorySuccess();
                return ApplicationConstants.RepositoryFailed();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error has occured");
                return ApplicationConstants.RepositoryFailed();
            }
        }

        public async Task<List<MenuSetupDTO>> GetMenu(string RoleID)
        {
            try
            {
                int startCount = 0;
                var response = await _context.MenuSetup.Where(c => c.RoleId == RoleID).Select(c => new MenuSetupDTO
                {
                    RoleId = c.RoleId,
                    ParentMenuId = c.ParentMenuId,
                    MenuId = c.MenuId,
                    MenuName = c.MenuName,
                    MenuUrl = c.MenuUrl,
                    IsActive = c.IsActive,
                    RoleName = c.RoleName,
                    IconClass = c.IconClass,
                    MenuSetupId = c.MenuSetupId,
                    IsSubMenu = c.IsSubMenu,
                    ModuleName = c.ModuleName,
                    IsGeneral = c.IsGeneral
                }).ToListAsync();
                response.ForEach(c =>
                {
                    c.MenuSetupId = startCount++;
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error occured while getting a Menu");
                return new List<MenuSetupDTO>();
            }
        }

        public async Task<List<MenuSetupDTO>> GetMenuForAssigningToUser(string RoleID)
        {
            try
            {
                int startCount = 0;
                var response = await _context.MenuSetup.Where(c => c.RoleId == RoleID && (c.ParentMenuId == null || string.IsNullOrEmpty(c.ParentMenuId) || c.IsSubMenu == true)).Select(c => new MenuSetupDTO
                {
                    RoleId = c.RoleId,
                    ParentMenuId = c.ParentMenuId,
                    MenuId = c.MenuId,
                    MenuName = c.MenuName,
                    MenuUrl = c.MenuUrl,
                    IsActive = c.IsActive,
                    RoleName = c.RoleName,
                    IconClass = c.IconClass,
                    MenuSetupId = c.MenuSetupId,
                    IsSubMenu = c.IsSubMenu,
                    ModuleName = c.ModuleName,
                    IsGeneral = c.IsGeneral
                }).ToListAsync();
                response.ForEach(c =>
                {
                    c.MenuSetupId = startCount++;
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error occured while getting a Menu");
                return new List<MenuSetupDTO>();
            }
        }

        public async Task<MerchantConfigs> GetMerchantSetup(string url)
        {
            MerchantConfigs result = new MerchantConfigs();
            try
            {
                var entity = await _context.MerchantConfig
          .FirstOrDefaultAsync(x => x.BaseUrl.ToLower() == url.ToLower());

                if (entity == null)
                    return null;

                return new MerchantConfigs
                {
                    BaseUrl = entity.BaseUrl,
                    Name = entity.Name
                    // Map other properties
                };


            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public async Task<List<MenuSetupDTO>> GetPagesAssignedToUser(string UserID, string userPageSecret, string userroleid)
        {
            try
            {
                var roleName = _authenticatedUser.RoleName;
                var response = new List<MenuSetupDTO>();

                if (_authenticatedUser.RoleName != Enum.GetName(typeof(Roles), Roles.GlobalAdmin) || _authenticatedUser.RoleName != Enum.GetName(typeof(Roles), Roles.AgencyAdmin))
                {
                    //if (userPageSecret != null && userPageSecret != "")
                    //{
                    //    var result = await _context.UsersRolePermission
                    //                   .Where(c => c.UserId == UserID && c.IsActive == true && c.IsDeleted == false)
                    //                   .Include(c => c.MenuSetup)
                    //                 .Select(c => new MenuSetupDTO
                    //                 {
                    //                     IconClass = c.MenuSetup.IconClass,
                    //                     IsActive = c.IsActive,
                    //                     MenuId = c.MenuSetup.MenuId,
                    //                     MenuName = c.MenuSetup.MenuName,
                    //                     MenuSetupId = c.MenuSetupId,
                    //                     MenuUrl = c.MenuSetup.MenuUrl,
                    //                     UserId = c.UserId,
                    //                     RoleId = c.MenuSetup.RoleId,
                    //                     ModuleName = c.MenuSetup.ModuleName,
                    //                 }).ToListAsync();
                    //    return result;
                    //}
                    var activePagesAssignedToUser = await _context.UsersRolePermission.Where(c => c.UserId == UserID).ToListAsync();
                    if (activePagesAssignedToUser.Count > 0)
                    {
                        foreach (var item in activePagesAssignedToUser)
                        {
                            var menuDetail = await _context.MenuSetup.FirstOrDefaultAsync(c => c.MenuSetupId == item.MenuSetupId);
                            if (menuDetail.ParentMenuId != null)
                            {
                                var parentMenu = await _context.MenuSetup.FirstOrDefaultAsync(c => c.MenuId == menuDetail.ParentMenuId);
                                if (parentMenu != null)
                                {
                                    if (!response.Any(c => c.MenuId == parentMenu.MenuId))
                                    {
                                        //include the parent
                                        response.Add(new MenuSetupDTO
                                        {
                                            MenuSetupId = parentMenu.MenuSetupId,
                                            MenuName = parentMenu.MenuName,
                                            MenuId = parentMenu.MenuId,
                                            IsActive = parentMenu.IsActive,
                                            MenuUrl = parentMenu.MenuUrl,
                                            ParentMenuId = parentMenu.ParentMenuId,
                                            UserId = UserID,
                                            IconClass = parentMenu.IconClass,
                                            ModuleName = parentMenu.ModuleName,
                                        });
                                    }
                                }
                                response.Add(new MenuSetupDTO
                                {
                                    MenuSetupId = menuDetail.MenuSetupId,
                                    MenuName = menuDetail.MenuName,
                                    MenuId = menuDetail.MenuId,
                                    MenuUrl = menuDetail.MenuUrl,
                                    IsActive = menuDetail.IsActive,
                                    ParentMenuId = menuDetail.ParentMenuId,
                                    UserId = UserID,
                                    IconClass = menuDetail.IconClass,
                                    ModuleName = menuDetail.ModuleName,
                                });
                            }
                            else
                            {
                                response.Add(new MenuSetupDTO
                                {
                                    MenuSetupId = menuDetail.MenuSetupId,
                                    MenuName = menuDetail.MenuName,
                                    MenuId = menuDetail.MenuId,
                                    IsActive = menuDetail.IsActive,
                                    MenuUrl = menuDetail.MenuUrl,
                                    ParentMenuId = menuDetail.ParentMenuId,
                                    UserId = UserID,
                                    IconClass = menuDetail.IconClass,
                                    ModuleName = menuDetail.ModuleName,
                                });
                            }
                        }
                    }
                    else
                    {
                        if (_authenticatedUser.RoleName == Enum.GetName(typeof(Roles), Roles.GlobalAdmin) || _authenticatedUser.RoleName == Enum.GetName(typeof(Roles), Roles.AgencyAdmin))
                        {
                            response = await _context.MenuSetup.Where(c => c.RoleId == userroleid).Select(c => new MenuSetupDTO
                            {
                                RoleId = c.RoleId,
                                ParentMenuId = c.ParentMenuId,
                                MenuId = c.MenuId,
                                MenuName = c.MenuName,
                                MenuUrl = c.MenuUrl,
                                IsActive = c.IsActive,
                                RoleName = c.RoleName,
                                IconClass = c.IconClass,
                                MenuSetupId = c.MenuSetupId,
                                IsSubMenu = c.IsSubMenu,
                                ModuleName = c.ModuleName
                            }).ToListAsync();
                        }
                    }
                    //Add General Pages
                    var generalpages = await _context.MenuSetup.Where(x => x.IsGeneral == true || x.RoleName.ToLower().Trim() == "General".ToLower().Trim()).ToListAsync();
                    foreach (var item in generalpages)
                    {
                        var generalMenuDto = new MenuSetupDTO
                        {
                            MenuSetupId = item.MenuSetupId,
                            MenuName = item.MenuName,
                            MenuId = item.MenuId,
                            MenuUrl = item.MenuUrl,
                            IsActive = item.IsActive,
                            ParentMenuId = item.ParentMenuId,
                            UserId = UserID,
                            IconClass = item.IconClass,
                            ModuleName = item.ModuleName,
                            IsGeneral = item.IsGeneral,
                        };
                        var rv = item;
                        var queryResponse = response.AsQueryable();

                        if (!queryResponse.Where(x => x.MenuSetupId == item.MenuSetupId).Any())
                        {
                            if (item.MenuName == "Dashboard" && item.RoleName == _authenticatedUser.RoleName)
                            {
                                response.Add(generalMenuDto);
                            }
                            else if (item.MenuName == "Dashboard" && item.RoleName != _authenticatedUser.RoleName)
                            {
                                //do not add
                            }
                            else
                            {
                                response.Add(generalMenuDto);
                            }
                        }
                    }

                    return response;
                }
                int startCount = 0;
                response = await _context.MenuSetup.Where(c => c.RoleId == userroleid).Select(c => new MenuSetupDTO
                {
                    RoleId = c.RoleId,
                    ParentMenuId = c.ParentMenuId,
                    MenuId = c.MenuId,
                    MenuName = c.MenuName,
                    MenuUrl = c.MenuUrl,
                    IsActive = c.IsActive,
                    RoleName = c.RoleName,
                    IconClass = c.IconClass,
                    MenuSetupId = c.MenuSetupId,
                    IsSubMenu = c.IsSubMenu,
                    ModuleName = c.ModuleName,
                    IsGeneral = c.IsGeneral
                }).ToListAsync();
                response.ForEach(c =>
                {
                    c.MenuSetupId = startCount++;
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while creating menu");
                return new List<MenuSetupDTO>();
            }
        }

        public bool MenuExistForUser(string UserId)
        {
            return (_context.UsersRolePermission.Where(x => x.UserId == UserId).Any());

        }

        public async Task<int> AssignMenusToUser(MenuToUserRequest model)
        {
            try
            {
                var existingpermission = await _context.UsersRolePermission.Where(x => x.UserId == model.UserId).ToListAsync();
                foreach (var item in existingpermission)
                {
                    _context.UsersRolePermission.Remove(item);
                    await _context.SaveChangesAsync();
                }

                var counter = 0;
                foreach (var menu in model.Menus)
                {
                    var menuIdentity = await _context.MenuSetup.FirstOrDefaultAsync(c => c.MenuId == menu);
                    var IsMenuAssignedPreviously = await _context.UsersRolePermission.AnyAsync(c => c.MenuSetupId == menuIdentity.MenuSetupId && c.UserId == model.UserId && c.IsDeleted == false);
                    if (!IsMenuAssignedPreviously)
                    {
                        var userRolePermission = new UsersRolePermission
                        {
                            MenuSetupId = menuIdentity.MenuSetupId,
                            UserId = model.UserId,
                            IsActive = true,
                            CreatedOn = DateTime.Now,
                            CreatedBy = model.UserId
                        };
                        await _context.UsersRolePermission.AddAsync(userRolePermission);
                        var result = await _context.SaveChangesAsync();
                        if (result > 0)
                        {
                            string action = $"Assigned {menuIdentity.MenuName} to {model.UserId}";
                            await _auditRepository.CreateAudit(model.UserId, action);
                        }
                        counter++;
                    }
                    else
                    {
                        //counter--;
                    }
                }

                return counter;
            }
            catch (Exception ex)
            {

                return 0; ;
            }
        }
        public async Task<int> DeleteMenuAssignedToUser(MenuToUserRequest model)
        {
            try
            {
                var assignedMenus = await _context.UsersRolePermission
                        .Where(c => c.IsActive && c.IsDeleted == false && model.Menus
                        .Contains(c.MenuSetup.MenuId))
                        .ToListAsync();
                _context.UsersRolePermission.RemoveRange(assignedMenus);
                var response = await _context.SaveChangesAsync();
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while deleting menu assign to users");
                return 0;
            }
        }

        public async Task<List<MenuSetupDTO>> GetMenusWithApprovalSettingAsTrue()
        {
            var response = await _context.MenuSetup.Where(c => c.ParentMenuId != "*" && c.RequiresApproval == true).Select(c => new MenuSetupDTO
            {
                RoleId = c.RoleId,
                ParentMenuId = c.ParentMenuId,
                MenuId = c.MenuId,
                MenuName = c.MenuName,
                MenuUrl = c.MenuUrl,
                IsActive = c.IsActive,
                RoleName = c.RoleName,
                MenuSetupId = c.MenuSetupId,
                ModuleName = c.ModuleName,
                IsGeneral = c.IsGeneral

            }).ToListAsync();
            return response;
        }

        public async Task<List<MenuSetupDTO>> GetAllMenu()
        {
            var response = await _context.MenuSetup.Select(c => new MenuSetupDTO
            {
                RoleId = c.RoleId,
                ParentMenuId = c.ParentMenuId,
                MenuId = c.MenuId,
                MenuName = c.MenuName,
                MenuUrl = c.MenuUrl,
                IsActive = c.IsActive,
                RoleName = c.RoleName,
                IconClass = c.IconClass,
                MenuSetupId = c.MenuSetupId,
                IsSubMenu = c.IsSubMenu,
                ModuleName = c.ModuleName,
                IsGeneral = c.IsGeneral
            }).ToListAsync();
            return response;
        }

        public async Task<Response<string>> UpdateMenu(UpdateMenuDTO request)
        {
            try
            {
                var menu = await _context.MenuSetup.FirstOrDefaultAsync(x => x.MenuSetupId == request.MenuSetupId);
                if (menu != null)
                {

                    menu.MenuId = request.MenuId;
                    menu.MenuName = request.MenuName;
                    menu.ParentMenuId = request.ParentMenuId;
                    menu.MenuUrl = request.MenuUrl;
                    menu.IsActive = request.IsActive;
                    menu.IsSubMenu = request.IsSubMenu;
                    menu.IconClass = request.IconClass;
                    menu.RoleName = request.RoleName;
                    menu.RequiresApproval = request.RequiresApproval;
                    menu.RoleId = request.RoleId;
                    menu.ModuleName = request.ModuleName;
                    menu.IsGeneral = request.IsGeneral;
                    _context.MenuSetup.Update(menu);
                    var save = await _context.SaveChangesAsync();
                    if (save > 0)
                    {
                        return ApplicationConstants.SuccessMessage("Update Successful");
                    }
                }
                return ApplicationConstants.NotFoundMessage("Menu not found");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Response<string>> DeleteMenu(int MenuSetupId)
        {
            try
            {
                var menu = await _context.MenuSetup.FirstOrDefaultAsync(x => x.MenuSetupId == MenuSetupId);
                if (menu != null)
                {
                    _context.MenuSetup.Remove(menu);
                    var save = await _context.SaveChangesAsync();
                    if (save > 0)
                    {
                        return ApplicationConstants.SuccessMessage("Delete Successful");
                    }
                }
                return ApplicationConstants.NotFoundMessage("Menu not found");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
