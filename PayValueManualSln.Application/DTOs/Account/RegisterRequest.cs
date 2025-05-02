using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Account
{
    public class RegisterRequest
    {
        [Required]
        public string RoleId { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string UserName { get; set; }
        public int ApprovalRankingId { get; set; }
        public string MerchantCode { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
    }
}
