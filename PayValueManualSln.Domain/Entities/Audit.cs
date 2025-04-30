using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{
    public partial class Audit
    {
        [Key]
        public int AuditId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
    }
}
