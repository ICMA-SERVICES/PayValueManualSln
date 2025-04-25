using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class UpdateIndividualPayerRequestDto
    {
        public string courtesyTitle { get; set; }
        public string surname { get; set; }
        public string firstName { get; set; }
        public string otherName { get; set; }
        public string utin { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public int revenueOfficeID { get; set; }
        public int lgaId { get; set; }
        public int townId { get; set; }
        public int businessTypeId { get; set; }
    }
}
