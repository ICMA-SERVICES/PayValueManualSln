using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Account
{
    public class MapInitiatorRequestDto
    {
        public bool? IsApprovalGeneral { get; set; }
        public List<InitiatorValidatorId> InitiatorValidatorId { get; set; }
    }

    public class InitiatorValidatorId
    {
        public string ValidatorId { get; set; }
        public string InitiatorId { get; set; }
    }
}
