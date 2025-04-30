using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayValueManualSln.Domain.Common;

namespace PayValueManualSln.Domain.Entities.Settings
{

    [Table("Range", Schema = "Setting")]
    public class RateRange : BaseEntity
    {
        [Key]
        public long Id { get; set; }
        public long? RateId { get; set; } = null;
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }
        public decimal? RateValue { get; set; }
        public string AgencyCode { get; set; }
        public bool? IsMultiplyRange { get; set; }

        public Rate Rate { get; set; }
    }
}
