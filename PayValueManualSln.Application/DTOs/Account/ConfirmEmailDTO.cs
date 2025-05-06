using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Account
{
    public class ConfirmEmailDTO
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
    }
}
