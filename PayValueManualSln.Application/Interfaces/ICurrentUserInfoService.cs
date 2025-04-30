using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
    public interface ICurrentUserInfoService
    {
        Task<string> GetUserFullNameAsync(string userId);
    }

}
