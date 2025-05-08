using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class GetDepositResponseDto
    {
        public string PaymentReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public string Error { get; set; }
    }
}
