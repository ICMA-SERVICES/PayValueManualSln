using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("RenewalFrequency", Schema = "Setting")]
    public class RenewalFrequency
    {
        public RenewalFrequency()
        {
            ServiceRevenues = new HashSet<ServiceRevenue>();
        }

        [Key]
        public int FrequencyId { get; set; }
        public int? FrequencyValue { get; set; }
        public string Description { get; set; }

        public ICollection<ServiceRevenue> ServiceRevenues { get; set; }
    }
}
