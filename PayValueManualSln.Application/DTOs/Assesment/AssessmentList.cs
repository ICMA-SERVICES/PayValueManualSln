using PayValueManualSln.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class AssessmentList
    {
        public string Name { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? DateApproved { get; set; }
        public string PayerRefNo { get; set; }
        public string PayerName => Name;
        public string AssessementRefNo { get; set; }
        public DateTime? AssessedDate => DateSubmitted;
        public DateTime? ApprovedDate => DateApproved;
        public string PaymentCode { get; set; }
        public string AssessementType { get; set; }
        public string PayerId => PayerRefNo;
        public string AssessementPeriod { get; set; }
        public decimal? AssessmentBalance { get; set; }
        public string AgencyName { get; set; }
        public string AgencyCode { get; set; }
        public string AssesementNoticeFormulatedString => AppWebExtension.Base64UrlEncode(AssessementRefNo.TrimEnd() + "/" + AgencyCode.TrimEnd());
        public string StandardLetterFormulatedString => AppWebExtension.Base64UrlEncode(AssessementRefNo.TrimEnd());
        public decimal AssesementAmount { get; set; }
        public string StatusName { get; set; }
    }
}
