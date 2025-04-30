using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("ServiceMethodSetups", Schema = "Setting")]
    public class ServiceMethodSetup
    {
        [Key]
        public long Id { get; set; }
        public long ServiceId { get; set; }
        public long TypeId { get; set; }
        public string ServiceMethodCode { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MinimumLandSize { get; set; }
        public decimal ActualRate { get; set; }
    }
}
