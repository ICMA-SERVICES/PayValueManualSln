using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
	public interface IEntityManager
	{
		Task<Response<List<ServicesDto>>> GetServicesAsync();
		Task<Response<List<RevenueDto>>> GetRevenueAsync();
		Task<ResponseDto> InsertAssessmentDataToBillTablesAsync(int assessmentId);
		

	}
}
