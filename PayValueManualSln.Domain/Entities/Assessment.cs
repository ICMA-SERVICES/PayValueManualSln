using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{

	[Table("Assessment", Schema = "dbo")]
	public class Assessment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Required]
		public int AssessmentID { get; set; }
		[MaxLength(50)]
		public string AssessmentRefNo { get; set; }
		[MaxLength(100)]
		public string TransactionId { get; set; }
		[MaxLength(50)]
		public string PaymentCode { get; set; }
		[MaxLength(50)]
		public string ItemPaymentCode { get; set; }
		[MaxLength(50)]
		public string ItemTransactionId { get; set; }
		[MaxLength(50)]
		public string MerchantCode { get; set; }
		[MaxLength]
		public string Location { get; set; }
		[MaxLength(150)]
		public string PayerName { get; set; }
		[MaxLength(20)]
		public string Telephone { get; set; }
		[MaxLength]
		public string Address { get; set; }
		public decimal? Amount { get; set; }
		public decimal? Arrears { get; set; }
		public string? TotalAmount { get; set; }
		[MaxLength]
		public string Narration { get; set; }
		[MaxLength(100)]
		public string RevenueCode { get; set; }
		[MaxLength(100)]
		public string RevenueName { get; set; }
		[MaxLength(100)]
		public string AgencyCode { get; set; }
		[MaxLength(100)]
		public string AgencyName { get; set; }
		[MaxLength(50)]
		public string Platformcode { get; set; }
		public string? AmountPaid { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public decimal? AssessmentBalance { get; set; }
		public bool? PartPaymentAllow { get; set; }
		public bool? AsExpiryDate { get; set; }
		public bool? IsExpired { get; set; }
		public long? ParentID { get; set; }
		public DateTime? DateCreated { get; set; }
		public bool? IsReversed { get; set; }
		[MaxLength(500)]
		public string Reversedby { get; set; }
		public DateTime? DateReversed { get; set; }
		[MaxLength(100)]
		public string AgentUtin { get; set; }
		[MaxLength(10)]
		public string TaxYear { get; set; }
		[MaxLength(50)]
		public string PreviousYearAssessmentRefNo { get; set; }
		[MaxLength(200)]
		public string AssessmentCreatedBy { get; set; }
		public DateTime? AssessmentCreatedDate { get; set; }
		[MaxLength(200)]
		public string AssessmentApprovedBy { get; set; }
		public DateTime? AssessmentDateApproved { get; set; }
		public bool? PushtoXpress { get; set; }
		public DateTime? DatePushtoXpress { get; set; }
		[Required]
		public bool IsPushedToPayersLedger { get; set; }
		[MaxLength(100)]
		public string PaymentPeriod { get; set; }

	}
}
