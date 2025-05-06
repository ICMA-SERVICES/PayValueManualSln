using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class GenerateInvoiceRequest
    {
        #region Public Properties
        [Required]
        public string TransactionId { get; set; }

        [Display(Name = ("Previous Year AssessmentRefNo"))]
        public string PreviousYearAssessmentRefNo { get; set; }

        [Required]
        public string PayerId { get; set; }

        [Required]
        [Display(Name = ("Payer Name"))]
        public string PayerName { get; set; }

        //[EmailAddress]
        [Display(Name = ("Payer Email"))]
        public string PayerEmail { get; set; }

        [Display(Name = ("Payer Telephone"))]
        public string Telephone { get; set; }

        [Display(Name = ("Payment Period"))]
        public string PaymentPeriod { get; set; }

        [Required]
        [Display(Name = ("Payer Address"))]
        public string Address { get; set; }

        [Required]
        [Display(Name = ("Invoice Total Amount"))]
        public decimal TotalAmount { get; set; }

        [Display(Name = ("Invoice Arrears"))]
        public decimal? Arrears { get; set; }

        [Display(Name = ("Invoice Description"))]
        public string Narration { get; set; }

        [Required]
        [Display(Name = ("Revenue Code"))]
        public string RevenueCode { get; set; }

        [Required]
        [Display(Name = ("Invoice Raised Location"))]
        public string Location { get; set; }

        [Required]
        [Display(Name = ("Invoice Year"))]
        public string InvoiceYear { get; set; }

        [Required]
        [Display(Name = ("Platform Code"))]
        public string PlatformCode { get; set; }

        [Required]
        [Display(Name = ("Invoice Raised By"))]
        public string RaisedBy { get; set; }

        [Required]
        [Display(Name = ("Invoice Raised On"))]
        public DateTime? RaisedOn { get; set; }

        [Required]
        [Display(Name = ("Invoice Approved By"))]
        public string ApprovedBy { get; set; }

        [Required]
        [Display(Name = ("Invoice Approved On"))]
        public DateTime? ApprovedOn { get; set; }

        [Required]
        [Display(Name = ("Update Previous Invoice"))]
        public bool UpdatePreviousInvoice { get; set; }

        [Display(Name = ("Payment Code"))]
        public string PaymentCode { get; set; }

        [Display(Name = ("Item Count"))]
        public int ItemCount { get; set; }
        public bool AsExpiryDate { get; set; } = false;
        public List<InvoiceDetails> InvoiceDetails { get; set; }
        #endregion
    }

    public class GenerateInvoiceResponse
    {
        public string TransactionId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentLink { get; set; } = string.Empty;
        public string PrintInvoiceLink { get; set; } = string.Empty;
        public List<InvoiceDetailsResponse> InvoiceDetailsResponse { get; set; }
    }
    public class InvoiceDetailsResponse
    {
        public string RevenueCode { get; set; }
        public decimal? ItemAmount { get; set; }
        public string ItemInvoiceNo { get; set; }
    }

    public class InvoiceDetails
    {
        #region Public Properties
        //[Required]
        public string ItemPaymentCode { get; set; }
        public string ItemTransactionId { get; set; }
        [Required]
        public string RevenueCode { get; set; }
        [Required]
        public decimal ItemAmount { get; set; }
        public decimal? ItemArrears { get; set; }
        public string Narration { get; set; }
        #endregion

    }
}
