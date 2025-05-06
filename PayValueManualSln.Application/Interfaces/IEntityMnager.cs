using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.Enums;
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
        Task<Response<string>> ApproveAssessmentAsync(AssessmentApprovalRequest request);
        Task<Response<List<BillInfoDto>>> GetAllAssessmentsAsync(int? year, AssessmentStatus assessmentStatus = AssessmentStatus.All);
        string GenerateBaseNumber(string merchantCode);
        Task<Response<string>> CalculateRenewalDate(long? billDetailId = null);
        Task<Response<List<BillInfoDto>>> GetAssessmentPendingApprovalByUserIdAsync();
        Task<Response<List<PayerDetailsDto>>> GetPendingAssessmentAsync();
		Task<Response<List<PayerDetailsDto>>> GetPendingAssessmentByRequesterIdAsync();
		Task<Response<ViewPendingAssessmentDto>> ViewPendingAssessment(string payerUtin);
		Task<Response<List<GetRateResponseDto>>> GetRevenuesForAssessmentAsync(GetRateRequestDto request);
        Task<Response<string>> RenewAssessments(long? billDetailId = null, bool? isManualRenewal = false);
        Task<Response<CreateAssessmentRequestDto>> CreateAssessment(CreateAssessmentRequestDto request);
		Task<Response<List<MapServiceToTypeRequestDto>>> MapServiceToType(List<MapServiceToTypeRequestDto> request);
        #region Rebate
        Task<MessageClass> GenerateExternalPaymentCode(string baseNumber, bool? updateBill = false, CancellationToken cancellationToken = default);
        Task<MessageClass> GenerateExternalPaymentCodeForPendingAssessment(CancellationToken cancellationToken = default);
        Task<MessageClass> WithDrawExternalPaymentCodeAsync(string formerBaseNumber, CancellationToken cancellationToken = default);
        #endregion
        #region Rebate
        Task<Response<string>> SendToAssessmentRepository(string baseNumber = null);
        #endregion
    }
}
