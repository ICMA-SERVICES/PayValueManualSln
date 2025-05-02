using PayValueManualSln.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("MapUserApproval", Schema = "Setting")]

    public class MapUserApproval : BaseEntity
    {
        [Key]
        public long Id { get; set; }
        public string InitiatorId { get; set; }
        public string ValidatorId { get; set; }
        public string AuthorizerId { get; set; }
        public string AgencyCode { get; set; }
    }
}
