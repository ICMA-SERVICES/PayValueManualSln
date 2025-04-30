using PayValueManualSln.Application.DTOs;
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
        Task<Response<PayerCollectionResponse>> GetAssessmentDetailAsync(string searchParam);
        Task<Response<PayerCollectionResponse>> ApprovePayerDetailAsync(UpdatePayerRequest request);
		Task<Response<bool>> SendPayerDetialToAdminAsync(UpdatePayerRequest request);
        Task<Response<List<AdditionalServiceDetailDto>>> GetAdditionalServiceDetail();
		Task<string> GenerateStinAsync(string username, int id);
        string GenerateBaseNumber(string merchantCode);
        Task<Response<List<PayerDetailsDto>>> GetPendingAssessmentAsync();
		Task<Response<List<PayerDetailsDto>>> GetPendingAssessmentByRequesterIdAsync();
		Task<Response<ViewPendingAssessmentDto>> ViewPendingAssessment(string payerUtin);
		Task<Response<List<GetRateResponseDto>>> GetRevenuesForAssessmentAsync(GetRateRequestDto request);
        Task<Response<CreateAssessmentRequestDto>> CreateAssessment(CreateAssessmentRequestDto request);
		Task<Response<List<MapServiceToTypeRequestDto>>> MapServiceToType(List<MapServiceToTypeRequestDto> request);
    }
}
