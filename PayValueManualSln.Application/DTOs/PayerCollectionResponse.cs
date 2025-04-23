using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class Data
    {
        public int totalCount { get; set; }
        public List<PayerCollectionDetail> payerCollectionDetails { get; set; }
    }

    public class PayerCollectionDetail
    {
        public string taxPayerReferenceNumber { get; set; }
        public string payerUtin { get; set; }
        public string surname { get; set; }
        public object firstName { get; set; }
        public string otherName { get; set; }
        public DateTime? dateofBirth { get; set; }
        public string email { get; set; }
        public string regTypeCode { get; set; }
        public DateTime? dateCreated { get; set; }
        public string address { get; set; }
        public object address1 { get; set; }
        public object address2 { get; set; }
        public object photograph { get; set; }
        public object contentType { get; set; }
        public object fileName { get; set; }
        public object signature { get; set; }
        public object courtesyTitle { get; set; }
        public string genderType { get; set; }
        public string phoneNo { get; set; }
        public object phoneNo1 { get; set; }
        public object phoneNo2 { get; set; }
        public object phoneNo3 { get; set; }
        public object jtbTin { get; set; }
        public string employeeName { get; set; }
        public string taxAgentReferenceNumber { get; set; }
        public string revenueOfficeName { get; set; }
        public object staffNumber { get; set; }
        public int? classOfEmployeeId { get; set; }
        public string classOfEmployeeName { get; set; }
        public string payerType { get; set; }
        public string payerCategory { get; set; }
        public string payerName { get; set; }
        public string fullPayerName { get; set; }
        public string utin { get; set; }
        public string contactPersonEmail { get; set; }
        public string telephoneNumber { get; set; }
        public string merchantCode { get; set; }
        public int? lgaId { get; set; }
        public string lgaName { get; set; }
        public string cacNumber { get; set; }
        public string revenueOfficeID { get; set; }
        public string contactName { get; set; }
        public bool? isPramary { get; set; }
        public bool? isParent { get; set; }
        public object businessTypeId { get; set; }
        public int? townID { get; set; }
    }

    public class PayerCollectionResponse
    {
        public int? totalCount { get; set; }
        public List<PayerCollectionDetail> payerCollectionDetails { get; set; }
    }
}
