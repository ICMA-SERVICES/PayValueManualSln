using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("ValueTemplateForLocation", Schema = "Setting")]
    public class ValueTemplateForLocation : BaseEntity
    {
        [Key]
        public long Id { get; set; }
        public long LocationId { get; set; }
        public long TypeId { get; set; }
        public decimal? Amount { get; set; }
        public bool? IsApproved { get; set; }
        public string AgencyCode { get; set; }

        public Location Location { get; set; }
        public Types Types { get; set; }
    }
}
