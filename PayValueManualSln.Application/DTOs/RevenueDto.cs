using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
	public class RevenueDto
	{
		public string RevenueCode { get; set; }
		public string RevenueName { get; set; }
		public long ServiceId { get; set; }
		public string PaymentItemName { get; set; }
		public string FormulaeValue { get; set; }
		public bool? IsAdditionalInfoRequired { get; set; }
		public bool? IsPaymentCodeEnabled { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? LastUpdated { get; set; }
		public string UpdatedBy { get; set; }
		public bool? IsDeleted { get; set; }
		public DateTime? DeletedOn { get; set; }
		public string DeletedBy { get; set; }



	}
}
