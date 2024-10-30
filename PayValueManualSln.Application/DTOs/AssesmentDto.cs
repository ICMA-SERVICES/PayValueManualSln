using PayValueManualSln.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
	public class AssesmentDto
	{
		public string AssessmentRefNo { get; set; }
		public string PayerName { get; set; }
		public string Telephone { get; set; }
		public string Address { get; set; }
		public string TotalAmount { get; set; }
		public string AssessmentAmount { get; set; }
		public string Narration { get; set; }
		public string RevenueCode { get; set; }
		public string RevenueName { get; set; }
		public string AgencyCode { get; set; }
		public string AgencyName { get; set; }
		public string AmountPaid { get; set; }
		public string AssessmentBalance { get; set; }
		public decimal? BillBalance { get; set; }
		public DateTime? DateCreated { get; set; }
		public bool? IsReversed { get; set; }
		public long? ParentID { get; set; }
		public string Reversedby { get; set; }
		public DateTime? DateReversed { get; set; }
		public string AgentUtin { get; set; }
		public string TaxYear { get; set; }
		public bool? IsExpired { get; set; }
		public string Platformcode { get; set; }
		public string AssessedPeriod { get; set; }
		public string PreviousYearAssessmentRefNo { get; set; }
		public string Status
		{
			get
			{
				return IsReversed == true ? "Reversed" : IsReversed == false ? "Active" : "Awaiting Approval";

			}
		}
	}
}
