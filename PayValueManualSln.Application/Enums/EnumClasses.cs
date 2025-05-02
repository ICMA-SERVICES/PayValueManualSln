using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Enums
{
    public enum PayerType
    {
        Ind = 1,
        Agen = 2
    }
    public enum InputDefinitionEnum
    {
        [Description("Pages")]
        Page = 1,
        [Description("Value")]
        Value = 2,
        [Description("Land Size")]
        LandSize = 3,
    }
    public enum Roles
    {
        GlobalAdmin,
        Initiator,
        Validator,
        Authorizer,
        AgencyAdmin,
        ICMAAdmin
    }
    public enum Status
    {
        INSERT = 1,
        UPDATE = 2,
        DELETE = 3,
        GETALL = 4,
        GETBYID = 5,
        GETMENUBYUSERID = 6,
        GETMENUBYROLEID = 7,
        GETSUBID = 8,
        GetMenuAssignedToUser = 9,
        GETBYPHONE = 10,
        Enable = 11,
        Disable = 12,
        Filter = 13
    }

    public enum ApprovalStatusEnum
    {
        Created = 1,
        Validated = 2,
        Authorized = 3,
        Completed = 4,
        Rejected = 5
    }

    public enum SummaryReportStatus
    {
        SummaryReport = 1,
        SummaryDetails = 2,
        DefaultersHistory = 3,
        PayerList = 4,
    }

    public enum CreditNoteRequestTypeEnum
    {
        Automatic = 1,
        Manual = 2
    }

    public enum TableSource
    {
        Assessment = 1,
        Collection = 2,
        Reconciliation = 3,
        CreditNote = 4
    }
    public enum PlatformName
    {
        CMBS = 1,
        TSS = 2,
        PayValue = 3
    }

    public enum AppId
    {
        CreditNote = 1,
        AssessmentPaymentNormalization = 2,
        PaymentSplitting = 3
    }

    public enum SplitPaymentTable
    {
        Temporary = 1,
        Original = 2
    }

    public enum TableType
    {
        Logo = 1,
        Signature = 2
    }

    public enum OperationType
    {
        Type = 1,
        Department,
        ServiceMethod,
        ServiceDetail,
        MapDepartment,
        ServiceRevenue,
        Zone,
        Category,
        Range,
        Input,
        InputDefinitionMapping,
        ValueTemplate,
        Rate
    }

    public enum TypeOfMap
    {
        UserToService = 1,
        UserToApproval,
        ServiceToType
    }

    public enum ServiceMethodCodes
    {
        MULTX1 = 1,
        MULTX2 = 2
    }

    public enum RenewalFrequencyEnum
    {
        Annual = 1,
        BiAnnual = 2
    }
    public enum AssessmentStatus
    {
        All = 1,
        Approved,
        Disapproved,
        Pending,
        PendingPayCode,
        Expired
    }

    public enum ReversalStatus
    {
        All = 1,
        ForValidator,
        ForInitiatorPending,
        ForInitiatorDissapproved,
        ForInitiatorApproved,
        Approved,
        Disaproved
    }
    public enum AdditionalServiceDetailDefinitions
    {
        [Description("Salutation")]
        Salutation = 1,
        [Description("Property Location")]
        PropertyLocation,
        [Description("Assignee (Property Owner)")]
        AssigneeOrPropertyOwner,
        [Description("File No.")]
        FileNo,
        [Description("Size of Land")]
        SizeOfLand,
        [Description("Terms of Grant")]
        TermsOfGrant,
        [Description("Property OwnerEmail")]
        PropertyOwnerEmail
    }
}
