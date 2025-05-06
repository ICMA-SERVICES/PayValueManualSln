using Microsoft.AspNetCore.Identity;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Infrastructure.Identity.Models;
using System.Threading.Tasks;

namespace PayValueManualSln.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.GlobalAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Initiator.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Validator.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Authorizer.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.AgencyAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.ICMAAdmin.ToString()));
        }
    }
}
