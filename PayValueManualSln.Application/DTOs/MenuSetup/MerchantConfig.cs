using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.MenuSetup
{
    public partial class MerchantConfigs
    {
        public MerchantConfigs()
        {
            Agency = new HashSet<AgencyDto>();
        }

        public int MerchantConfigId { get; set; }
        public string MerchantCode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public Guid Guid { get; set; }
        public string Logo { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string BgImage { get; set; }
        public string Color { get; set; }
        public string BaseUrl { get; set; }
        public string StateFooter { get; set; }
        public string MerchantWebSite { get; set; }
        public string MerchantPhone { get; set; }
        public string MerchantPhone1 { get; set; }
        public string MerchantEmail { get; set; }
        public string MerchantAddress { get; set; }
        public string MerchantAddress2 { get; set; }

        public string DepositOnConsentRevenue { get; set; }
        public string AgencyPhoneNo { get; set; }
        public string AgencyEmail { get; set; }
        public int? LandSize { get; set; }
        public int? PaymentForGovernorsConsentOnDeedOfAssignment { get; set; }
        public int? PaymentForLandAllocation { get; set; }
        public int? PaymentForStateLandRevalidationOnTitle1 { get; set; }
        public int? PaymentForStateLandRevalidationOnTitle2 { get; set; }
        public int? PaymentForLandAccommodation { get; set; }
        public int? PaymentForRatification { get; set; }
        public string StateName { get; set; }

        public int? AppModuleModuleId { get; set; }
        public bool IsManualAssessment { get; set; } = false;

        public virtual AppModule AppModuleModule { get; set; }
        public virtual ICollection<AgencyDto> Agency { get; set; }
    }
}

