using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Application.DTOs.Account;
using PayValueManualSln.Application.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress);
        Task<Response<string>> RegisterAsync(RegisterRequest request);
        Task<Response<string>> ConfirmEmailAsync(string userId, string code, string password, bool bypass = false);
        Task<Response<string>> ForgotPassword(ForgotPasswordRequest model);
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);
        Task<Response<string>> ChangePassword(ChangePasswordRequest model);
        Task<Response<string>> ChangePassword(ChangePasswordRequestByPass model);
        Task<Response<string>> Logout();
        Task<Response<string>> ResendEmail(string email);
        Task<int> GetUserCount(string loggedInUserId);
        Task<List<UserDTO>> GetUsers(string loggedInUserId);
        Task<List<UserDTO>> GetUsers(string loggedInUserId, int count);
        Task<dynamic> GetUsersByRole(Roles roles);
        Task<List<UserDTO>> GetUsers();
        Task<List<UserDTO>> GetUserForAgencyAdmin(string AgencyCode);
        Task<Response<string>> RemoveUser(string userId);

        Task<Response<List<RolesDTO>>> GetRoles();
        Response<UserDTO> GetUserById(string userid);
        Task<int> Enable(string userId);
        Task<int> Disable(string userId);
        Task UpdateUser(UpdateUser updateUserDto);
        Task<Response<string>> MapInitiatorToValidator(MapInitiatorRequestDto request);
    }
}
