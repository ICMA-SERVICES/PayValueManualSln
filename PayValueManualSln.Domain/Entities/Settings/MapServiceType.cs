using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("MapServiceTypes", Schema = "Setting")]
    public class MapServiceType : BaseEntity
    {
        public long Id { get; set; }
        public long TypeId { get; set; }
        public string TypeName { get; set; }
        public long ServiceId { get; set; }
    }
}
