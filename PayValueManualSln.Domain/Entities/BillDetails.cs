using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayValueV2.Domain.Entities.PayValue
{
	[Table("BillDetails", Schema = "dbo")]
	public class BillDetails
	{
		public int Id { get; set; }
		public Guid BillInfoGuid { get; set; }
		public long? BillInfoId { get; set; }
		public long? RateId { get; set; }
		public long? ServiceId { get; set; }
		public string ServiceName { get; set; }
		public long? TypeId { get; set; }
		public string TypeName { get; set; }
		public string BaseNumberItemRefNo { get; set; } //Victor you remove this, its redant now.
		public long? ZoneId { get; set; }
		public string ZoneName { get; set; }
		public int? LocationId { get; set; }
		public string LocationName { get; set; }
		public bool? IsDepositRequired { get; set; }
		public string ItemPaymentCode { get; set; }
		public string PaymentReferenceNum { get; set; }
		public string PaymentItemName { get; set; }
		public long? ServiceRevenueId { get; set; }
		public decimal BillAmount { get; set; } = 0;
		public decimal? Liability { get; set; } = 0;
		public bool? IsRebated { get; set; } = false;
		public decimal? RebatePercentage { get; set; }
		public decimal? RebateAmount { get; set; } = 0;
		public Guid? RebateRequestId { get; set; }
		public bool? ShowRebateAmount { get; set; }
		public string? TotalBillAmount { get; set; }
		public decimal BillAmountPaid { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public decimal? BillBalance { get; set; } = 0;
		public string RevenueCode { get; set; }
		public string RevenueName { get; set; }
		public bool? PartPaymentAllow { get; set; } = true;
		public string AgencyCode { get; set; }
		public string AgencyName { get; set; }
		public DateTime? CreatedOn { get; set; } = DateTime.Now;
		public string CreatedById { get; set; }
		public DateTime? UpdateOn { get; set; }
		public int? UpdateById { get; set; }
		public bool? IsReversed { get; set; }
		public BillInfo BillInfo { get; set; }
	}
}
