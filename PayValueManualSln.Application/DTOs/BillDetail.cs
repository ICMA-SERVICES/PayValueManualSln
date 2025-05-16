using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    public class BillDetail
    {
        //public int? Id { get; set; }
        public long? RateId { get; set; }
        public long? ServiceId { get; set; }
        public long? ServiceRevenueId { get; set; }
        public string ServiceName { get; set; }
        public long? TypeId { get; set; }
        public string TypeName { get; set; }
        public long? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public string PaymentItemName { get; set; }
        public decimal BillAmount { get; set; }
        public decimal? Liability { get; set; }
        public string RevenueCode { get; set; }
        public string RevenueName { get; set; }
        public string PaymentReferenceNum { get; set; }
        public bool? IsDepositRequired { get; set; }

        //PROPERTIES USED BY HANGFIRE FOR RENEWAL
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public string CreatedById { get; set; }
        public string PreviousBaseNumber { get; set; } //Update for new record
        public string PreviousItemPaymentCode { get; set; } //Update for new record
    }
}
