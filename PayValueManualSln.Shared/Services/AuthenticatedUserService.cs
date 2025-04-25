using Microsoft.AspNetCore.Http;
using PayValueManualSln.Application.Interfaces;
using System.Security.Claims; // Ensure this namespace is included

namespace PayValueManualSln.Shared.Services
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Email = httpContextAccessor.HttpContext?.User?.FindFirst("useremail")?.Value;
            UserName = httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;
            RoleId = httpContextAccessor.HttpContext?.User?.FindFirst("roleid")?.Value;
            RoleName = httpContextAccessor.HttpContext?.User?.FindFirst("rolename")?.Value;
            RankId = httpContextAccessor.HttpContext?.User?.FindFirst("rankid")?.Value;
            MerchantCode = httpContextAccessor.HttpContext?.User?.FindFirst("merchantcode")?.Value;
            Name = httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value;
            AgencyCode = httpContextAccessor.HttpContext?.User?.FindFirst("AgencyCode")?.Value;
            AgencyName = httpContextAccessor.HttpContext?.User?.FindFirst("AgencyName")?.Value;
        }

        public string UserId { get; }
        public string Email { get; }
        public string UserName { get; }
        public string RankId { get; }
        public string RoleId { get; }
        public string RoleName { get; }
        public string MerchantCode { get; }
        public string Name { get; }
        public string AgencyCode { get; }
        public string AgencyName { get; }
    }
}
