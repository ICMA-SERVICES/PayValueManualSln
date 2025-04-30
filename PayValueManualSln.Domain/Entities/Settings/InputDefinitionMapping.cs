using PayValueManualSln.Domain.Common;
using PayValueManualSln.Domain.Entities.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("InputDefinitionMapping", Schema = "Setting")]
    public class InputDefinitionMapping : BaseEntity
    {
        [Key]
        public long Id { get; set; }
        public long InputId { get; set; }
        public long ServiceId { get; set; }
        public string AgencyCode { get; set; }

        public Services Services { get; set; }
    }
}
