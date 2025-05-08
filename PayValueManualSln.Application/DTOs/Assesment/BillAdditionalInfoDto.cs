using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class BillAdditionalInfoDto
    {
        public long AdditionalInfoId { get; set; }
        public string RequestId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public int AdditionalServiceDetailId { get; set; }
        public string AdditionalServiceDetailName { get; set; }
        public Guid BillInfoId { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
    }
}
