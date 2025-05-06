using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Domain.Entities
{
    [Table("CollectionRevenue", Schema = "Collection")]
    public partial class CollectionRevenue
    {
        [Key]
        public long RevenueId { get; set; }
        public string RevenueCode { get; set; }
        public string RevenueName { get; set; }
        public Int32 AgencyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsBIR { get; set; }
        public bool? isInternal { get; set; }
        public decimal MinimunAmount { get; set; }
        public bool? BillValidationRequired { get; set; }
        public bool? PayerValidationRequired { get; set; }
    }

    [Table("CollectionAgency", Schema = "Collection")]
    public partial class CollectionAgency
    {
        [Key]
        public Int32 AgencyId { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public string MerchantCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public string ShortName { get; set; }
        public bool? IsBIR { get; set; }
        public bool? isInternal { get; set; }

    }
}
