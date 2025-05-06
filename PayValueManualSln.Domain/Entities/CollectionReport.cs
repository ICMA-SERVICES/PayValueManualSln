using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{
    public partial class CollectionReport
    {
        public long PaymentId { get; set; }
        public string MerchantCode { get; set; }
        public string ColProviderCode { get; set; }
        public string ChannelCode { get; set; }
        public string PaymentRefNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }
        public decimal AmountUsed { get; set; }
        public decimal PaymentBalance { get; set; }
        public string PayerName { get; set; }
        public string PayerId { get; set; }
        public string PayerAddress { get; set; }
        public string DepositSlipNumber { get; set; }
        public bool? IsDateExtended { get; set; }
        public string TelephoneNumber { get; set; }
        public string PaymentMethodCode { get; set; }
        public string PaymentValueStatusCode { get; set; }
        public string ValueStatus { get; set; }
        public DateTime? ValueDate { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string IcmaReceipt { get; set; }
        public DateTime? IcmaReceiptDate { get; set; }
        public long RevenueId { get; set; }
        public string RevenueCode { get; set; }
        public string RevenueName { get; set; }
        public int AgencyId { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public int? BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string ChequeNumber { get; set; }
        public string ChequeBankCode { get; set; }
        public string ChequeBankName { get; set; }
        public string PostedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsReversed { get; set; }
        public DateTime? ReversedDate { get; set; }
        public string ReversalId { get; set; }
        public string TransactionReference { get; set; }
        public string AssessmentNo { get; set; }
        public string ControlNumber { get; set; }
        public string PaymentPeriod { get; set; }
        public string Exception { get; set; }
        public bool? IsModified { get; set; }
        public bool? IsUsed { get; set; }
        public bool? IsSplitted { get; set; }
        public string PayerUtin { get; set; }
        public string NormalisedBy { get; set; }
        public DateTime? NormalisedDate { get; set; }
        public int? UserId { get; set; }
        public string PlatformCode { get; set; }
        public decimal? Balance { get; set; }
        public int? StateCode { get; set; }
        public bool IsPaymentPeriodSplitted { get; set; }
        public bool IsPushedToPlatformOwner { get; set; }
        public string FinalUtin { get; set; }
        public bool? IsRepositoryUpdate { get; set; }
        public string CurrencyCode { get; set; }
        public string UsedByPlatform { get; set; }
    }
}
