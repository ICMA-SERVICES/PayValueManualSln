using PayValueManualSln.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class BillInfoDto
    {
        public long Id { get; set; }
        public Guid BillId { get; set; }
        public string PayerName { get; set; }
        public string PayerUtin { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public decimal? TotalAssessed { get; set; }
        public decimal? Liability { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string DissaprovalComment { get; set; }
        public decimal? TotalBillAmount { get; set; }
        public string PaymentCode { get; set; }
        public bool? IsAdditionalAssessmentRequired { get; set; }
        public bool? IsPrimaryAssessment { get; set; }
        public bool? IsStandardLetterRequired { get; set; } = false;
        public string BillPeriod { get; set; }
        public Guid? MergerRequestId { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
        public bool? IsApproved { get; set; } = false;
        public int? NoOfApprovalCount { get; set; }
        public int? NoOfApprovedCount { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public bool? IsReversed { get; set; }
        public string ReversedBy { get; set; }
        public DateTime? ReversedOn { get; set; }
        public bool? IsUpdated { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? SignatureId { get; set; }
        public bool? IsMailSent { get; set; }
        public bool? IsMailSendingRequired { get; set; }
        public bool? IsRebated { get; set; }
        public int? ClientId { get; set; }
        public double? landSize { get; set; }
        public double? Pages { get; set; }
        public string EntityLocation { get; set; }
        public string Assignee { get; set; }
        public string FileNo { get; set; }
        public string TermOfGrant { get; set; }
        public string PropertyOwnerEmail { get; set; }
        public string Salutation { get; set; }
        public decimal? Value { get; set; }
        public string CreatedBy { get; set; }
        public string TypeName { get; set; }
        public string Location { get; set; }
        public string BaseNumber { get; set; }
        public string AgencyCode { get; set; }
        public string Zone { get; set; }
        public string AssessmentNoticeFormulatedString
        {
            get
            {
                if (BaseNumber != null && AgencyCode != null)
                    return AppWebExtension.Base64UrlEncode(BaseNumber.TrimEnd() + "/" + AgencyCode.TrimEnd());
                else
                    return null;
            }
        }

        public string StandardLetterFormulatedString
        {
            get
            {
                if (BaseNumber != null)
                    return AppWebExtension.Base64UrlEncode(BaseNumber.TrimEnd());
                else
                    return null;
            }
        }

      //  public List<BillDetails> BillDetails { get; set; }
    }
}
