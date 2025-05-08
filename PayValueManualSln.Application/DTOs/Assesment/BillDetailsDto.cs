using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class BillDetailsDto
    {
        public int Id { get; set; }
        public Guid BillInfoGuid { get; set; }
        public long? BillInfoId { get; set; }
        public long? RateId { get; set; }
        public long? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public long? TypeId { get; set; }
        public string TypeName { get; set; }
        public long? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public string ItemPaymentCode { get; set; }
        public string PaymentItemName { get; set; }
        public long? ServiceRevenueId { get; set; }
        public decimal? BillAmount { get; set; }
        public decimal? Liability { get; set; }
        public bool? IsRebated { get; set; } = false;
        public decimal? RebatePercentage { get; set; }
        public decimal? RebateAmount { get; set; }
        public Guid? RebateRequestId { get; set; }
        public bool ShowRebateAmount { get; set; } = false;
        public decimal? TotalBillAmount { get; set; }
        public decimal? BillAmountPaid { get; set; } = 0;
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? BillBalance { get; private set; }
        public string RevenueCode { get; set; }
        public string RevenueName { get; set; }
        public bool? PartPaymentAllow { get; set; } = true;
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedById { get; set; }
        public DateTime? UpdateOn { get; set; }
        public int? UpdateById { get; set; }
        public bool? IsReversed { get; set; }
        public string AssesementAmountFormatted => Convert.ToDecimal(Convert.ToDecimal(BillAmount).ToString(CultureInfo.InvariantCulture)).ToString("N");
        public string AssessmentBalanceFormatted => Convert.ToDecimal(Convert.ToDecimal(BillBalance).ToString(CultureInfo.InvariantCulture)).ToString("N");
        public string RebateAmountFormatted => Convert.ToDecimal(Convert.ToDecimal(RebateAmount).ToString(CultureInfo.InvariantCulture)).ToString("N");
    }
}
