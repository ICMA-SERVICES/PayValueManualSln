using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class UpdateBillAdditionalInfoRequestDto
    {
        public long AdditionalInfoId { get; set; }
        public int AdditionalServiceDetailId { get; set; }
        public Guid BillInfoId { get; set; }
        public string FieldValue { get; set; }
    }
}
