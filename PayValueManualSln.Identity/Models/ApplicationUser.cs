using Microsoft.AspNetCore.Identity;
using PayValueManualSln.Application.DTOs.Account;
using System;
using System.Collections.Generic;

namespace PayValueManualSln.Infrastructure.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? IsActive { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public string? DisabledBy { get; set; }
        public string? EnabledBy { get; set; }
        public DateTime? DateEnabled { get; set; }
        public DateTime? DateDisabled { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public string? DeletedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }
        public string MerchantCode { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public int? ApprovalRankingId { get; set; }
        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}
