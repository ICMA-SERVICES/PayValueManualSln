using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class GetDepositRequestDto
    {
        public string PaymentRefNumber { get; set; }
        public string PayerUtin { get; set; }
        //public string RevenueCode { get; set; }
    }
}
