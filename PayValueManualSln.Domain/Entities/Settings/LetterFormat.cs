using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    [Table("LetterFormat", Schema = "Setting")]
    public class LetterFormat
    {
        public int Id { get; set; }
        public string HeaderName { get; set; }
        public string AgencyCode { get; set; }
        public bool IsAddressIncluded { get; set; }
    }
}
