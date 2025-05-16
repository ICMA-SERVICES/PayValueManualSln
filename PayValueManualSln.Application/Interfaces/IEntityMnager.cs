using Microsoft.AspNetCore.Mvc;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Helpers;
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
        Task<Response<BillInfoDto>> GetAssessmentsByIdAsync(long billInfoId);
        Task<string> GenerateStinAsync(string username, int id);
        Task<Response<string>> ApproveAssessmentAsync(AssessmentApprovalRequest request);
        Task<Response<string>> UpdateBaseNumber(long billId, string baseNumber);
        Task<Response<List<GetDepositResponseDto>>> GetDepositAmount([FromQuery] List<GetDepositRequestDto> request);
        Task<Response<List<BillInfoDto>>> GetAllAssessmentsAsync(int? year, AssessmentStatus assessmentStatus = AssessmentStatus.All);
        string GenerateBaseNumber(string merchantCode);
        public string EmptyPdf();
        public string AssessmentEmptyPdf();
        Task<List<AssessmentList>> GetAllApprovedAssessmentsAsync(DataSourceLoadOptions loadOptions, string agencyCode);
        string GetConsumptionTaxAssessmentNoticeHtml(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, string howToPayUrl, string confirmDocUrl);
        Task<Response<string>> UpdateBillAdditionalInfo(List<UpdateBillAdditionalInfoRequestDto> request);
        Task<byte[]> ToPdf(PdfConverterRequest request);
        Task<List<BillAdditionalInfoDto>> GetAdditionalInfoByBillInfoId(Guid billInfoGuid);
        Task<Response<List<AdditionalServiceDetailDto>>> GetAllAdditionalServiceDetailName();
        string GetPaymentInstructionHTMLStringAsync();
        string GetCertificateOfOccupancyHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        Task<List<AssessmentDTO>> GetAllApprovedAssessmentNotice(string sentAgencyCode, string sentAssRefNo);
        string GetAccomodationHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        string GetRatificationOfLandTitleHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        Task<List<AssessmentDTO>> GetAllAssessmentDetails(string decodedStringAssRefNo);
        string GetRevalidationOfLandTitleHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        string GetGovernorConsentHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        Task<string> GenerateConfirmDocBarcode(string barCodeRequest);
        string GetAllocationHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        string GetAssessmentNoticeHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, string howToPayUrl, string confirmDocUrl);
        Task<string> GenerateBarcode(string urlPath);
        Task<List<BillAdditionalInfoDto>> GetAllAdditionalInfo(Guid billInfoGuid);
        Task<List<BillDetailsDto>> GetBillDetail(Guid billInfoGuid);
        string GetAssessmentsNoticeHtmlString(List<AssessmentsDto> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl);
        Task<List<AssessmentsDto>> GetAllApprovedAssessmentNotices(string sentAgencyCode, string sentAssRefNo);
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
