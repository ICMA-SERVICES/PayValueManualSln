using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public partial class ModuleApprovalConfig
    {
        [Key]
        public int ModuleApprovalId { get; set; }
        public bool? IsApprovalForGeneralValidators { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string AgencyCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public int? NoOfCheckersRequired { get; set; }
        public int? NoOfAuthorizersRequired { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsApprovalSequence { get; set; }
        public int? ModuleId { get; set; }
    }
}
