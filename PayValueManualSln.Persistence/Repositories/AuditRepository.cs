using MediatR;
using Microsoft.AspNetCore.Identity;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
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
        private readonly ICurrentUserInfoService _userInfoService;

        public AuditRepository(ApplicationDbContext context, ICurrentUserInfoService userInfoService)
        {
            _context = context;
            _userInfoService = userInfoService;
        }

        public async Task CreateAudit(string userId, string action)
        {
            var fullName = await _userInfoService.GetUserFullNameAsync(userId);

            var newAudit = new Audit
            {
                Action = action,
                UserId = userId,
                UserFullName = fullName,
                Date = DateTime.Now
            };

            await _context.Audit.AddAsync(newAudit);
            await _context.SaveChangesAsync();
        }
    }

}

