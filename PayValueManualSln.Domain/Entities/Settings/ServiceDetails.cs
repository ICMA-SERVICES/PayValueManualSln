using PayValueManualSln.Domain.Common;
using PayValueManualSln.Domain.Entities.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class ServiceDetails : BaseEntity
    {
        [Key]
        public long Id { get; set; }
        //public string Name { get; set; }
        public long ServiceId { get; set; }
        public bool? IsActive { get; set; } = true;
        public string DetailName { get; set; }
        public long AdditionalInfoId { get; set; }

        public virtual BillAdditionalInfo BillAdditionalInfo { get; set; }
        
    }
}
