using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class MapServiceToTypeRequestDto 
    {
        public long Id { get; set; }
        public long TypeId { get; set; }
        public string TypeName { get; set; }
        public long ServiceId { get; set; }
        public string CreatedBy { get; set; }
    }
}
