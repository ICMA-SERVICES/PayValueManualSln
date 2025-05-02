using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Account
{
    public class AuthenticationResponse
    {
        public string Id { get; set; }
        //public string CompanyCode { get; set; }
        //public string CompanyName { get; set; }
        //public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string StaffId { get; set; }
        public List<string> Roles { get; set; }
        public List<string> RolesId { get; set; }
        public string UserRolesId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool? IsActive { get; set; }
        public string JWToken { get; set; }
        public DateTime DateCreated { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
        public string MerchantCode { get; set; }
        public int? ApprovalRankingId { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public bool IsManualAssessment { get; set; }
    }
}
