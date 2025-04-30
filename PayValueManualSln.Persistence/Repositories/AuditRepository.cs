using MediatR;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Persistence.Services
{
    public class AuditRepository : IAuditRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditRepository(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task CreateAudit(string userId, string action)
        {
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var newAudit = new Audit
                {
                    Action = action,
                    UserId = userId,
                    UserFullName = user.FirstName + " " + user.LastName,
                    Date = DateTime.Now
                };
                await _context.Audit.AddAsync(newAudit);
                await _context.SaveChangesAsync();
            }
        }
    }
}
}
