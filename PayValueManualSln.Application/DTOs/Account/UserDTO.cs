using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Account
{
    public class UserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public bool? IsDeleted { get; set; }
        public string CompanyCode { get; set; }
        public int CompanyId { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public bool IsFirstLogin { get; set; }
        public string Status { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public int ApprovalRankingId { get; set; }
        public DateTime DateCreated { get; set; }
        public string MerchantCode { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
    }
}
