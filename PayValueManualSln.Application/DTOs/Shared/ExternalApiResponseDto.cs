using System.Collections.Generic;

namespace PayValueManualSln.Application.Dtos.Shared
{
    public class ExternalApiResponseDto
    {
        public int StatusId { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
        public object errors { get; set; }
        public Dictionary<string, object> data { get; set; }
    }

    public class SendAssessmentToRepositoryResponseData
    {
        public string TransactionId { get; set; }
        public int TotalAmount { get; set; }
        public string InvoiceNo { get; set; }
        public List<InvoiceDetailsResponse> InvoiceDetailsResponse { get; set; }
    }

    public class InvoiceDetailsResponse
    {
        public string RevenueCode { get; set; }
        public int ItemAmount { get; set; }
        public string ItemInvoiceNo { get; set; }
    }

    public class ExternalApiResponseDto<T> where T : class
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
        public T Data { get; set; }
    }


}
