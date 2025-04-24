using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{
    [Table("AdditionalServiceDetail")]
    public class AdditionalServiceDetail
    {
        [Key]
        public int Id { get; set; }
        public string AdditionalServiceDetailName { get; set; }
    }
}
