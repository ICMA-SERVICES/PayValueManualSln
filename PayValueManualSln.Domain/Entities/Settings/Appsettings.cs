using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities.Settings
{
    public class Appsettings
    {
        public int LandSize { get; set; }
        public string ServiceBaseUrl { get; set; }
        public string DefaultConnection { get; set; }
        public string DMPSLOGEVENTSConnection { get; set; }
        public string ApplicationBaseUrl { get; set; }
        public string DepositOnConsentRevenue { get; set; }
        public string AgencyPhoneNo { get; set; }
        public string AgencyEmail { get; set; }
        public string BarcodeServiceUrl { get; set; }
        public string BPMSGateWayServiceUrl { get; set; }
        public string MerchantCode { get; set; }
        public string StateLogo { get; set; }
        public int ConvertTypeId { get; set; }
        public string StateLogoUrl { get; set; }
        public string PdfConverterUrl { get; set; }
        public string SignatureUrl { get; set; }
        public string NoticeTitle { get; set; }
        public int PaymentForRatification { get; set; } = 0;
        public long? PaymentForGovernorsConsentOnDeedOfAssignment { get; set; } = 0;
        public long? PaymentForLandAllocation { get; set; } = 0;
        public long? ConsumptionTax { get; set; } = 0;
        public long? PaymentForStateLandRevalidationOnTitle1 { get; set; } = 0;
        public long? PaymentForStateLandRevalidationOnTitle2 { get; set; } = 0;
        public long? PaymentForLandAccommodation { get; set; } = 0;
        public int PaymentForCertificateOfOccupancy { get; set; } = 0;
        public string StateName { get; set; }
        public string AssessmentRepoServiceBaseUrl { get; set; }
        public string GenerateInvoiceNoMethod { get; set; }
        public string PlatformCode { get; set; }
        public string UnclassifiedRevenueCode { get; set; }

        //public string CMBSWebBaseUrl { get; set; }
        //public string BIRName { get; set; }
        //public string CMBSFullName { get; set; }
        public string PaymentPlatformNames { get; set; }
        public string DemandNoticeGracePeriod { get; set; }
        public string PaymentOnlineWebsite { get; set; }
        public string ComplaintEmail { get; set; }
        public string SelfServiceUrl { get; set; }
        public int NoticePeriodNumber { get; set; }
        public int ComplaintPeriod { get; set; }
        //public string PaymentPlatforms { get; set; }
        //public string PaymentOnlineUrl { get; set; }

    }
}

