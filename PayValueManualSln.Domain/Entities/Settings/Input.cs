using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{

    [Table("Input", Schema = "Setting")]
    public class Input : BaseEntity
    {
        public long Id { get; set; }
        public string InputName { get; set; }
        public string AgencyCode { get; set; }
    }
}
