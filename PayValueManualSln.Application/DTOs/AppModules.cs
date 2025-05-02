using PayValueManualSln.Application.DTOs.MenuSetup;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class AppModule /*: BaseEntity*/
    {
        public AppModule()
        {
            MerchantConfig = new HashSet<MerchantConfigs>();
        }
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

        public virtual ICollection<MerchantConfigs> MerchantConfig { get; set; }
    }
}
