using PayValueManualSln.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Account
{
    public class UpdateUserDTO
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int? ApprovalRankingId { get; set; }
        public Roles Role { get; set; }
    }


    public class UpdateUser
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public int ApprovalRankingId { get; set; }

        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
    }
}
