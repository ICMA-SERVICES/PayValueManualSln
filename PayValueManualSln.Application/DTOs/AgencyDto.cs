using PayValueManualSln.Application.DTOs.MenuSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
    
    public class AgencyDto
    {
        public string Code { get; set; }
        public int Id { get; set; }
        public string MerchantCode { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string OfficialAddress { get; set; }
        public string OfficialAddress1 { get; set; }
        public string OfficialAddress2 { get; set; }
        public string AuthorizedName { get; set; }
        public string AuthorizedPosition { get; set; }
        public string GovernorName { get; set; }
        public bool IsAutoRenewal { get; set; }
        public bool? ExternalPaymentCodeRequired { get; set; }
        public int AgencyLogoId { get; set; }
        public int AuthorizedSignatureId { get; set; }

        public virtual MerchantConfigs MerchantCodeNavigation { get; set; }
       
    }

}
