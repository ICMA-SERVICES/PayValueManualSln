using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
    public interface IAuthenticatedUserService
    {
        string UserId { get; }
        string Email { get; }
        string UserName { get; }
        string RankId { get; }
        string RoleId { get; }
        string RoleName { get; }
        string MerchantCode { get; }
        string Name { get; }
        string AgencyName { get; }
        string AgencyCode { get; }
    }
}
