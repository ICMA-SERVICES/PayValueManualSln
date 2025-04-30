using Microsoft.AspNetCore.Identity;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Identity
{
    public class CurrentUserInfoService : ICurrentUserInfoService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserInfoService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GetUserFullNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? "Unknown" : $"{user.FirstName} {user.LastName}";
        }
    }

}
