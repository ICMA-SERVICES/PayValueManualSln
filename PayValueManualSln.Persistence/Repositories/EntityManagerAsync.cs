using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueV2.Domain.Entities.PayValue;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayValueManualSln.Persistence.Repositories
{
	public class EntityMangerAsync : IEntityManager
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger _logger;
		private readonly IConfiguration _config;
		private readonly IHttpClientHelperService _httpClientHelperService;
		private readonly IMapper _mapper;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public EntityMangerAsync(ApplicationDbContext context, ILogger logger, IConfiguration config, IHttpClientHelperService httpClientHelperService,IMapper mapper,IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _httpClientHelperService = httpClientHelperService;
			_mapper = mapper;
            _authenticatedUser = authenticatedUserService;
        }


        public async Task<Response<List<ServicesDto>>> GetServicesAsync()
		{
			var response = new Response<List<ServicesDto>>();
			try
			{
				var result = await _context.Services
					.Where(a => !string.IsNullOrEmpty(a.Name))
					.OrderByDescending(c => c.CreatedOn)
					.Select(x => new ServicesDto
					{
						Name = x.Name ?? string.Empty,
					})
					.ToListAsync();

				response.Data = result.Select(s => new ServicesDto
				{
					Name = s.Name
				}).ToList();

				response.Succeeded = true;
				response.Message = "Services retrieved successfully.";
			}
			catch (Exception ex)
			{
				Log.Error(ex, "An error occurred in GetServicesAsync.");
				response.Succeeded = false;
				response.Message = $"An error occurred: {ex.Message}";
			}
			return response;
		}

		public async Task<Response<List<RevenueDto>>> GetRevenueAsync()
		{
			var response = new Response<List<RevenueDto>>();
			try
			{
				var result = await _context.Services
					.Where(a => !string.IsNullOrEmpty(a.Name))
					.OrderByDescending(c => c.CreatedOn)
					.Select(x => new RevenueDto
					{
						RevenueName = x.Name ?? string.Empty,
					})
					.ToListAsync();

				response.Data = result.Select(s => new RevenueDto
				{
					RevenueName = s.RevenueName
				}).ToList();

				response.Succeeded = true;
				response.Message = "Revenue retrieved successfully.";
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "An error occurred in GetRevenueAsync.");
				response.Succeeded = false;
				response.Message = $"An error occurred: {ex.Message}";
			}
			return response;
		}

		public async Task<ResponseDto> InsertAssessmentDataToBillTablesAsync(int assessmentId)
		{
			var response = new ResponseDto();
			try
			{
				var assessment = await _context.Assessment.FindAsync(assessmentId);
				if (assessment == null)
				{
					response.Succeeded = false;
					response.Message = "Assessment not found.";
					return response;
				}

				var billInfo = new BillInfo
				{
					PayerName = assessment.PayerName,
					Telephone = assessment.Telephone,
					Address = assessment.Address,
					TotalAssessed = assessment.TotalAmount,
					AgencyCode = assessment.AgencyCode,
					CreatedOn = assessment.DateCreated,
					AmountPaid = assessment.AmountPaid,
					TotalBillAmount = assessment.TotalAmount,
					CreatedById = assessment.AssessmentCreatedBy,
					IsReversed = assessment.IsReversed,
					ReversedBy = assessment.Reversedby,
					ReversedOn = assessment.DateReversed,
					IsExpired = assessment.IsExpired
				};

				await _context.BillInfo.AddAsync(billInfo);

				var billDetails = new BillDetails
				{
					RevenueCode = assessment.RevenueCode,
					RevenueName = assessment.RevenueName,
					AgencyCode = assessment.AgencyCode,
					AgencyName = assessment.AgencyName,
					CreatedOn = assessment.DateCreated,
					TotalBillAmount = assessment.TotalAmount,
					Liability = assessment.AssessmentBalance,
					PartPaymentAllow = assessment.PartPaymentAllow,
					CreatedById = assessment.AssessmentCreatedBy,
					IsReversed = assessment.IsReversed
				};

				await _context.BillDetails.AddAsync(billDetails);

				await _context.SaveChangesAsync();

				response.Succeeded = true;
				response.Message = "Assessment data successfully inserted into BillInfo and BillDetails.";
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "An error occurred in InsertAssessmentDataToBillTablesAsync.");
				response.Succeeded = false;
				response.Message = $"An error occurred: {ex.Message}";
			}

			return response;
		}
        public async Task<Response<PayerCollectionResponse>> GetAssessmentDetailAsync(string searchParam)
        {
            var response = new Response<PayerCollectionResponse>();
            try
            {
                var MerchantCode = _config.GetSection("ExternalLinks")["MerchantCode"];
                var baseUrl = _config.GetSection("ExternalLinks")["PayerCollectionDetailBaseUrl"];
                var endpointUrl = _config.GetSection("ExternalLinks")["PayerCollectionDetailEndpoint"];

                var requestModel = new ExternalPostRequestDTO
                {
                    SearchParam = searchParam
                };
                var json = JsonConvert.SerializeObject(requestModel);
                _logger.Information($"##External API REQUEST##: {json}");
                var externalUrl = $"{baseUrl}{endpointUrl}";
                var result = await _httpClientHelperService.GetAsync<ExternalApiResponseDto<PayerCollectionResponse>>(externalUrl, "", "", MerchantCode, searchParam);

                if (result != null && result.Data != null)
                {
                    response.Data = result.Data;
                    response.Succeeded = true;
                    response.Message = "Assessment details retrieved successfully.";
                    _logger.Information($"##External API Response##: {JsonConvert.SerializeObject(result)}");
                }
                else
                {
                    response.Succeeded = false;
                    response.Message = "No data found.";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred in GetAssessmentDetailAsync.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }

            return response;
        }
		public async Task<Response<PayerCollectionResponse>> ApprovePayerDetailAsync(UpdatePayerRequest request)
		{
            var response = new Response<PayerCollectionResponse>();
			try
			{
                var record = await _context.PayerDetails.FirstOrDefaultAsync(c => c.payerUtin.ToLower() == request.payerUtin.ToLower());

				if (record != null)
				{
					record.IsApproved = false;
					record.ApprovalComment = request.ApprovalComment;
					record.ActedUponOn = DateTime.Now;
					_context.PayerDetails.Update(record);
					await _context.SaveChangesAsync();
					response.Message = "Record has been dissapproved";
					response.Data = null;
					return response;
				}
				else
				{
					var MerchantCode = _config.GetSection("ExternalLinks")["MerchantCode"];
					var baseUrl = _config.GetSection("ExternalLinks")["PayerCollectionDetailBaseUrl"];
					var endpointUrl = request.payerType == PayerType.Ind.ToString() ? _config.GetSection("ExternalLinks")["UpdatePayerDetailForIndividualEndpoint"] : _config.GetSection("ExternalLinks")["UpdatePayerDetailForAgentEndpoint"];
					object payerRequest = new object();
					request.utin = string.IsNullOrEmpty(request.utin) ? request.payerUtin : request.utin;
					if (request.payerType == PayerType.Ind.ToString())
					{
						payerRequest = _mapper.Map<UpdateIndividualPayerRequestDto>(request);
					}
					else
					{
						payerRequest = _mapper.Map<UpdateAgentRequestDto>(request);
					}
					request.utin = string.IsNullOrEmpty(request.utin) ? request.payerUtin : request.utin;
					var json = JsonConvert.SerializeObject(payerRequest);
					_logger.Information($"##External API REQUEST##: {json}");
					var externalUrl = $"{baseUrl}{endpointUrl}";
					var result = await _httpClientHelperService.PostAsync<object, ExternalApiResponseDto<PayerCollectionResponse>>(externalUrl, json, "", MerchantCode, request.payerUtin);
					string errors = result == null ? "The remote server returned null response" : string.Join(",", result.Errors);
					if (result != null)
					{
						_logger.Information($"##External API Response##: {JsonConvert.SerializeObject(result)}");
						if (result.Succeeded)
						{
							record.IsApproved = true;
							record.ApprovalComment = request.ApprovalComment;
							record.ActedUponOn = DateTime.Now;
							_context.PayerDetails.Update(record);
							await _context.SaveChangesAsync();
							response.Message = "Approval was successful";
							response.Data = result.Data;
							return response;
						}
						else
						{
                            response.Succeeded = false;
                            response.Message = "Approval was Unsucessful";
                        }
						return response;
					}
				}

            }
			catch (Exception ex)
			{
                _logger.Error(ex, "An error occurred in ApprovePayerDetailAsync.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
			return response;
        }
		public async Task<Response<bool>> SendPayerDetialToAdminAsync(UpdatePayerRequest request)
		{
			var response = new Response<bool>();
			try
			{
              var mapper = _mapper.Map<PayerDetails>(request);
				mapper.ActedUponOn = DateTime.Now;
                mapper.ChangeRequesterId = _authenticatedUser.UserId;
				await _context.PayerDetails.AddAsync(mapper);
				var result = await _context.SaveChangesAsync();
                response.Message = result > 0
				? "Successful"
				: "UnSuccessful";
                response.Succeeded = result > 0;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred in SendPayerDetialToAdminAsync.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
		public async Task<Response<List<AdditionalServiceDetailDto>>> GetAdditionalServiceDetail()
		{
			var response = new Response<List<AdditionalServiceDetailDto>>();
			try
			{
                var list = await _context.AdditionalServiceDetail.ToListAsync();
                response.Message = (list != null && list.Any()) ? "Successful" : "Unsuccessful";
                response.Succeeded = list != null && list.Any();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred in GetAdditionalServiceDetail.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
			return response;
        }



    }
}
