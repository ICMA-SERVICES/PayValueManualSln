
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayValueV2.Domain.Entities.PayValue
{
	[Table("BillInfo", Schema = "dbo")]
	public class BillInfo 
	{
		public BillInfo()
		{
			BillDetails = new HashSet<BillDetails>();
		}
		public long Id { get; set; }
		public Guid BillId { get; set; }
		public string AgencyCode { get; set; }
		public long? ServiceId { get; set; }
		public string ServiceName { get; set; }
		public string PayerName { get; set; }
		public string PayerUtin { get; set; }
		public string Email { get; set; }
		public string Telephone { get; set; }
		public string Address { get; set; }
		public string TotalAssessed { get; set; }
		public decimal? Liability { get; set; }
		public string? TotalBillAmount { get; set; }
		public string? AmountPaid { get; set; }
		public string PaymentCode { get; set; }
		public bool? IsAdditionalAssessmentRequired { get; set; }
		public bool? IsPrimaryAssessment { get; set; }
		public string BillPeriod { get; set; }
		public string DissaprovalComment { get; set; }
		public Guid? MergerRequestId { get; set; }
		public string CreatedById { get; set; }
		public DateTime? CreatedOn { get; set; } = DateTime.Now;
		public bool? IsApproved { get; set; } = null;
		public int? NoOfApprovalCount { get; set; }
		public int? NoOfApprovedCount { get; set; }
		public string ApprovedBy { get; set; }
		public string ExternalResponseJson { get; set; }
		public DateTime? ApprovedOn { get; set; }
		public bool? IsReversed { get; set; }
		public string ReversedBy { get; set; }
		public DateTime? ReversedOn { get; set; }
		public bool? IsUpdated { get; set; }
		public string UpdatedById { get; set; }
		public DateTime? UpdatedOn { get; set; }
		public DateTime? ExternalResponseDate { get; set; }
		public int? SignatureId { get; set; }
		public bool? IsMailSent { get; set; }
		public bool? IsMailSendingRequired { get; set; }
		public bool? IsRebated { get; set; }
		public bool IsBillWithdrawn { get; set; } = false;
		public bool? IsSentToAssessmentRepository { get; set; } = false;
		public bool? IsExpired { get; set; } = false;
		public string ReasonForExpiration { get; set; }
		public DateTime? BillWithdrawnOn { get; set; }
		public int? ClientId { get; set; }
		public long AdditionalInfoId { get; set; }
		public string BaseNumber { get; set; }  //PVOG210AA031
		public double? LandSize { get; set; }
		public double? Pages { get; set; }
		public decimal? Value { get; set; }
		public virtual ICollection<BillDetails> BillDetails { get; set; }

		//public string EntityLocation { get; set; }
		//public string Assignee { get; set; }
		//public string FileNo { get; set; }
		//public string TermOfGrant { get; set; }
		//public string PropertyOwnerEmail { get; set; }
		//public string Salutation { get; set; }
	}
}