using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class GetRateRequestDto
    {
        public long? LocationId { get; set; }
        public long? ZoneId { get; set; }
        public long? ServiceId { get; set; }
        public double? LandSize { get; set; }
        public long? TypeId { get; set; }
        public decimal? Value { get; set; }
        public long? Pages { get; set; }
    }
}
