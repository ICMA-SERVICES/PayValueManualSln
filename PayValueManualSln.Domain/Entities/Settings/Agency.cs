using PayValueManualSln.Domain.Common;
using PayValueManualSln.Domain.Entities.Setting;
using PayValueManualSln.Domain.Entities.Settings;

public class Agency : BaseEntity
{
    public Agency()
    {
        Department = new HashSet<Department>();
        ServiceMethod = new HashSet<ServiceMethod>();
        Services = new HashSet<Services>();
    }

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
    public virtual AgencySignature AgencySignature { get; set; }
    public virtual AgencyLogo AgencyLogo { get; set; }
    public virtual ICollection<Department> Department { get; set; }
    public virtual ICollection<MerchantConfig> MerchantConfig { get; set; }
    public virtual ICollection<ServiceMethod> ServiceMethod { get; set; }
    public virtual ICollection<Services> Services { get; set; }
}
