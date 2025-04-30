using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class GetRateResponseDto
    {
        public long RateId { get; set; }
        public string RevenueName { get; set; }
        public string PayementItemName { get; set; }
        public decimal Amount { get; set; }
        public string Error { get; set; }
        public string RevenueCode { get; set; }
        public bool? IsDepositRequired { get; set; }
    }
}
