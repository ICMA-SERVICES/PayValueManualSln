using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{

    [Table("BillAdditionalInfo", Schema = "Assessment")]
    public class BillAdditionalInfo : BaseEntity
    {
        [Key]
        public long AdditionalInfoId { get; set; }
        public int AdditionalServiceDetailId { get; set; }
        public Guid BillInfoId { get; set; }
        public string FieldValue { get; set; }
    }
}
