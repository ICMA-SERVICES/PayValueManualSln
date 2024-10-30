using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
	public class ServicesDto
	{
		public string Name { get; set; }
		public string PaymentPeriod { get; set; }
		public bool ReminderRequired { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsApproved { get; set; }
		public bool IsValueInputRequired { get; set; }
		public bool IsServiceApplicableToAll { get; set; }
		public bool IsForAllZones { get; set; }
		public bool IsLiabilityRequired { get; set; }
		public bool IsManualAssessment { get; set; }
		public bool IsAdditionalAssessmentRequired { get; set; }
		public bool IsPrimaryAssessment { get; set; }
		public long? ExternalServiceId { get; set; }
		public long? DeptId { get; set; }
		public string Description { get; set; }
		public string ServiceHeader { get; set; }
		public string ServiceSubHeader { get; set; }
		public bool IsStandardLetterRequired { get; set; }
		public string StandardLetterDescriptions { get; set; }
		public string SignatoryName { get; set; }
		public string SignatoryPosition { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime LastUpdated { get; set; }
		public DateTime DeletedOn { get; set; }
		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }
		public string DeletedBy { get; set; }



	}
}
