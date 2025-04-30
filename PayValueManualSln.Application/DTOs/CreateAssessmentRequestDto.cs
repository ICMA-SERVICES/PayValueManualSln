using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class CreateAssessmentRequestDto
    {
        //public long? Id { get; set; }
        public string PayerName { get; set; }
        public string PayerUtin { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string PaymentCode { get; set; }
        public bool? IsAdditionalAssessmentRequired { get; set; }
        public bool? IsPrimaryAssessment { get; set; }
        public string BillPeriod { get; set; }
        public int? SignatureId { get; set; }
        public bool? IsMailSent { get; set; }
        public bool? IsMailSendingRequired { get; set; }
        public bool? IsRebated { get; set; }
        public int? ClientId { get; set; }
        public long? ServiceId { get; set; }
        public double Pages { get; set; }
        public string PropertyOwnerEmail { get; set; }
        public decimal? Value { get; set; }
        public double LandSize { get; set; }
        public List<AdditionalBillRequestDto> AdditionalBillInfo { get; set; }
        public List<BillDetail> BillDetails { get; set; }

        //PROPERTIES USED BY HANGFIRE FOR RENEWAL
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public string CreatedById { get; set; }
        public bool? IsApproved { get; set; }
        public string PreviousBaseNumber { get; set; } //this is for the newly renewed record
        public string PreviousPaymentCode { get; set; } //this is for the newly renewed record

        //public string Assignee { get; set; }
        //public string EntityLocation { get; set; }
        //public string Salutation { get; set; }
        //public string FileNo { get; set; }
        //public double landSize { get; set; }
        //public string TermOfGrant { get; set; }
    }
}
