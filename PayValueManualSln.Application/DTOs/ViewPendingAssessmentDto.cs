using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class ViewPendingAssessmentDto
    {
        public PayerDetailsDto NewRecord { get; set; }
        public PayerCollectionDetail OldRecord { get; set; }
    }
}
