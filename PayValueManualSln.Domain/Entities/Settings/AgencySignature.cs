using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("AgencySignature", Schema = "Setting")]
    public class AgencySignature
    {
        [Key]
        public int Id { get; set; }
        public string AgencyCode { get; set; }
        public string Image { get; set; }
        public string RentRevisionPeriod { get; set; }
        public bool? IsActive { get; set; }
        public Agency Agency { get; set; }
    }
}
