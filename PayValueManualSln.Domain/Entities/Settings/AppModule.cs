using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{

    [Table("Module", Schema = "Setting")]
    public class AppModule /*: BaseEntity*/
    {
        public AppModule()
        {
            MerchantConfig = new HashSet<MerchantConfig>();
        }

        [Key]
        public int ModuleId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual ICollection<MerchantConfig> MerchantConfig { get; set; }
    }
}
