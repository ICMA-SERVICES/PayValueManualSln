using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.Assesment
{
    public class BPMSPaymentCodeRequest
    {

        public string customer_first_name { get; set; }
        public string customer_last_name { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string bill_description { get; set; }
        public double billed_amount { get; set; }
        public bool overwrite_existing { get; set; }
        public int service_id { get; set; }
        public string payment_code { get; set; }
        public string request_id { get; set; }
        public List<DemandNoticeDetails> demand_notices { get; set; }
    }

    public class BPMSWithdrawBillRequest
    {
        public string payment_code { get; set; }
    }

    public class BPMSFreshPaymentCodeRequest
    {

        public string customer_first_name { get; set; }
        public string customer_last_name { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }
        public string customer_address { get; set; }
        public string bill_description { get; set; }
        public double billed_amount { get; set; }
        public bool overwrite_existing { get; set; }
        public int service_id { get; set; }
        public string request_id { get; set; }
        public List<DemandNoticeDetails> demand_notices { get; set; }
    }

    public class DemandNoticeDetails
    {
        public double amount { get; set; }
        public string name { get; set; }
        public string revenue_code { get; set; }
        public long ServiceId { get; set; }

    }

    public class RMRegisterDto
    {
        public bool UpdateBPMS { get; set; }
        public string PaymentCode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public DateTime? DateUpdateBPMS { get; set; }

    }

    public class DemandNotice
    {
        public string amount { get; set; }
        public string invoiced_amount { get; set; }
        public string amount_paid { get; set; }
        public string amount_remaining { get; set; }
        public string agency_name { get; set; }
        public string agency_code { get; set; }
        public string revenue_name { get; set; }
        public string revenue_code { get; set; }
        public string revenue_item_reference { get; set; }
    }

    public class Data
    {
        public string request_id { get; set; }
        public string payment_code { get; set; }
        public string payment_url { get; set; }
        public List<DemandNotice> demand_notices { get; set; }
    }

    public class BMPSMessageClass
    {
        public string message { get; set; }
        public Data data { get; set; }
        public bool status { get; set; }
        public int status_code { get; set; }
    }
}
