using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.RateDto;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Persistence.Helpers;
using PayValueManualSln.Persistence.Services;
using PayValueManualSln.Shared.DapperServices;
using PayValueV2.Domain.Entities.PayValue;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
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
        private readonly HttpClient _httpClient;
        private readonly RateServices _rateservice;
        private readonly IDapper _dapper;
        private readonly IAuditRepository _audit;
        public Appsettings _appsettings { get; }

        public EntityMangerAsync(ApplicationDbContext context, IAuditRepository audit,IOptions<Appsettings> appsettings, IDapper dapper ,ILogger logger, IConfiguration config, IHttpClientHelperService httpClientHelperService, IMapper mapper, IAuthenticatedUserService authenticatedUserService, HttpClient httpClient, RateServices rateServices)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _httpClientHelperService = httpClientHelperService;
            _mapper = mapper;
            _authenticatedUser = authenticatedUserService;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://services.ogunstaterevenue.com/");
            _rateservice = rateServices;
            _dapper = dapper;
            _appsettings = appsettings.Value;
            _audit = audit;
        }

        public async Task<string> GenerateStinAsync(string username, int id)
        {
            var endpoint = $"PayerRegistrationService/GenerateSTIN/api/Generate/STINGenerate/{username}/{id}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<ExternalApiResponseDto<PayerCollectionResponse>> CallUpdatePayerDetailAsync(object requestBody, string endpointUrl)
        {
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpointUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                // Optionally log or handle different status codes
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExternalApiResponseDto<PayerCollectionResponse>>(responseContent);
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
                    var externalUrl = $"{baseUrl}{"/PayerRegistration/int/retrive-payer-Information"}";
                    var result = await CallUpdatePayerDetailAsync(payerRequest, $"{externalUrl}");
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
        public async Task<Response<List<PayerDetailsDto>>> GetPendingAssessmentAsync()
        {
            var response = new Response<List<PayerDetailsDto>>();
            try
            {
                var result = await _context.PayerDetails.Where(x => x.IsApproved == null).ToListAsync();
                if (result == null || !result.Any())
                {
                    response.Succeeded = false;
                    response.Message = "No pending assessments found.";
                    return response;
                }
                response.Message = "Pending assessments retrieved successfully.";
                response.Data = result.Select(s => new PayerDetailsDto
                {
                    payerName = s.payerName,
                    payerUtin = s.payerUtin,
                    dateCreated = s.dateCreated,
                    email = s.email,
                    phoneNo = s.phoneNo,
                    address = s.address,
                    IsApproved = s.IsApproved
                }).ToList();

                response.Succeeded = true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred in GetPendingAssessment.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<Response<List<PayerDetailsDto>>> GetPendingAssessmentByRequesterIdAsync()
        {
            var response = new Response<List<PayerDetailsDto>>();
            try
            {
                var result = await _context.PayerDetails.Where(x => x.ChangeRequesterId == _authenticatedUser.UserId).ToListAsync();
                if (result == null || !result.Any())
                {
                    response.Succeeded = false;
                    response.Message = "No pending assessments id found.";
                    return response;
                }
                response.Message = "Pending assessments id retrieved successfully.";
                response.Data = result.Select(s => new PayerDetailsDto
                {
                    payerName = s.payerName,
                    payerUtin = s.payerUtin,
                    dateCreated = s.dateCreated,
                    email = s.email,
                    phoneNo = s.phoneNo,
                    address = s.address,
                    IsApproved = s.IsApproved
                }).ToList();

                response.Succeeded = true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred in GetPendingAssessment ID.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<Response<ViewPendingAssessmentDto>> ViewPendingAssessment(string payerUtin)
        {
            var response = new Response<ViewPendingAssessmentDto>();
            try
            {
                var newRecord = await _context.PayerDetails.FirstOrDefaultAsync(c => c.payerUtin.ToLower() == payerUtin.ToLower().Trim());
                var oldRecord = await GetAssessmentDetailAsync(payerUtin);
                if (newRecord != null && oldRecord.Data != null)
                {
                    var result = new ViewPendingAssessmentDto
                    {
                        NewRecord = _mapper.Map<PayerDetailsDto>(newRecord),
                        OldRecord = oldRecord.Data.payerCollectionDetails.FirstOrDefault()
                    };
                    response.Message = "Pending assessment retrieved successfully.";
                    response.Succeeded = true;
                }
                else
                {
                    response.Succeeded = false;
                    response.Message = "No pending assessment found.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred in ViewPendingAssessment.");
                response.Succeeded = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }

        public async Task<Response<List<GetRateResponseDto>>> GetRevenuesForAssessmentAsync(GetRateRequestDto request)
        {
            var response = new List<GetRateResponseDto>();
            try
            {
                var isForAllZones = await _rateservice.CheckIsForAllZones(request.ServiceId);
                var rates = await _rateservice.GetRate(request.ServiceId, request.LocationId, request.ZoneId, isForAllZones, request.TypeId);
                foreach (var rateItem in rates)
                {
                    var rate = new Rate();
                    //check if this revenue requires formula
                    if (rateItem.IsAmountAutomatic == false || rateItem.IsAmountAutomatic == null)
                    {
                        rate = await _context.Rate
                            .Include(x => x.ServiceRevenue)
                            .FirstOrDefaultAsync(x => x.ServiceId == request.ServiceId && x.TypeId == request.TypeId && x.LocationId == request.LocationId && x.ZoneId == request.ZoneId && x.IsApproved == true);
                    }
                    else
                    {
                        rate = await _context.Rate
                            .Include(x => x.ServiceRevenue)
                            .FirstOrDefaultAsync(x => x.Id == rateItem.Id && x.IsApproved == true);
                    }

                    if (rate != null && rate.ServiceMethodId > 0)
                    {
                        var calculationParameters = new CalculationParameters()
                        {
                            LandSize = request.LandSize,
                            LocationId = request.LocationId,
                            ServiceId = request.ServiceId,
                            ZoneId = request.ZoneId,
                            ServiceMethodId = rate.ServiceMethodId,
                            RateAmount = rate.Amount,
                            TypeId = request.TypeId,
                            RateId = rate.Id,
                            InputDefinitionId = rate.InputDefinitionId,
                            Pages = request.Pages,
                            IsServiceForAllLocation = isForAllZones,
                            Value = request.Value
                        };
                        var doCalculation = await _rateservice.CalculateRevenue(calculationParameters);
                        response.Add(new GetRateResponseDto
                        {
                            Amount = doCalculation,
                            RateId = rate.Id,
                            PayementItemName = rate.ServiceRevenue.PaymentItemName,
                            RevenueName = rate.RevenueName,
                            RevenueCode = rate.RevenueCode,
                            IsDepositRequired = rate.IsDepositRequired
                        });
                    }
                    else if (rate != null)
                    {
                        response.Add(new GetRateResponseDto
                        {
                            Amount = (Decimal.TryParse(rate.Amount, out decimal result)) ? Decimal.Parse(rate.Amount) : 0,
                            RateId = rate.Id,
                            RevenueCode = rate.RevenueCode,
                            PayementItemName = rate.ServiceRevenue.PaymentItemName,
                            RevenueName = rate.RevenueName,
                            IsDepositRequired = rate.IsDepositRequired
                        });
                    }
                    else if (rate == null)
                    {
                        response.Add(new GetRateResponseDto
                        {
                            Amount = 0,
                            RevenueName = $"Rate setup does not exist for this Revenue {rateItem.RevenueName}"
                        });
                    }
                }

                return new Response<List<GetRateResponseDto>>
                {
                    Data = response,
                    Succeeded = true,
                    Message = "Revenues retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                Log.Error(ex.Message, "An error has occurred on GetRevenuesForAssessmentAsync, PayValue Repository");

                return new Response<List<GetRateResponseDto>>
                {
                    Data = null,
                    Succeeded = false,
                    Message = "An error occurred while retrieving revenues."
                };
            }
        }

        public string GenerateBaseNumber(string merchantCode)
        {
            string baseNumber = string.Empty;
            try
            {
                var dbParams = new DynamicParameters();
                dbParams.Add("@MerchantCode", merchantCode, DbType.String);
                var result = _dapper.GetAll<string>($"[dbo].[GenerateBaseNumber]", dbParams, commandType: CommandType.StoredProcedure, _appsettings.DefaultConnection);

                baseNumber = result.FirstOrDefault();
                return baseNumber;
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message);
            }

            return baseNumber;
        }

        private bool IsRenewalRequired(long serviceRevenueId)
        {
            return _context.ServiceRevenue.Any(x => x.Id == serviceRevenueId && x.IsRenewable == true);
        }

        public async Task<Response<string>> CalculateRenewalDate(long? billDetailId = null)
        {
             var response = new Response<string>();
            try
            {
                var renewableAssessments = new List<AssessmentRenewalDto>();
                var currentYear = DateTime.Now.Year;

                if (billDetailId == null)
                {
                    renewableAssessments = await (from bd in _context.BillDetails
                                                  join sr in _context.ServiceRevenue on bd.ServiceRevenueId equals sr.Id
                                                  join rf in _context.RenewalFrequencies.DefaultIfEmpty() on sr.RenewalFrequencyId equals rf.FrequencyId
                                                  where sr.IsRenewable == true
                                                        && bd.RenewalDate == null
                                                        && bd.IsRenewed != true
                                                        && (bd.IsReversed == false || bd.IsReversed == null)
                                                  //&& bd.CreatedOn.Year == currentYear
                                                  select new AssessmentRenewalDto
                                                  {
                                                      IsRenewable = sr.IsRenewable,
                                                      AllowAutomaticTrigger = sr.AllowAutomaticTrigger,
                                                      AutomaticApproval = sr.AutomaticApproval,
                                                      RenewalFrequencyId = sr.RenewalFrequencyId,
                                                      IsRenewableByDate = sr.IsRenewableByDate,
                                                      BillDetailId = bd.Id,
                                                      AssessmentCreatedDate = bd.CreatedOn,
                                                      FrequencyValue = rf.FrequencyValue,
                                                      ServiceRevenueId = sr.Id
                                                  }).ToListAsync();
                }
                else
                {
                    renewableAssessments = await (from bd in _context.BillDetails
                                                  join sr in _context.ServiceRevenue on bd.ServiceRevenueId equals sr.Id
                                                  join rf in _context.RenewalFrequencies.DefaultIfEmpty() on sr.RenewalFrequencyId equals rf.FrequencyId
                                                  where sr.IsRenewable == true
                                                        && bd.RenewalDate == null
                                                        && bd.IsRenewed != true
                                                        && (bd.IsReversed == false || bd.IsReversed == null)
                                                        && bd.Id == (long)billDetailId
                                                  select new AssessmentRenewalDto
                                                  {
                                                      IsRenewable = sr.IsRenewable,
                                                      AllowAutomaticTrigger = sr.AllowAutomaticTrigger,
                                                      AutomaticApproval = sr.AutomaticApproval,
                                                      RenewalFrequencyId = sr.RenewalFrequencyId,
                                                      IsRenewableByDate = sr.IsRenewableByDate,
                                                      BillDetailId = bd.Id,
                                                      AssessmentCreatedDate = bd.CreatedOn,
                                                      FrequencyValue = rf.FrequencyValue,
                                                      ServiceRevenueId = sr.Id
                                                  }).ToListAsync();
                }

                if (renewableAssessments.Count > 0)
                {
                    var updateList = new List<BillDetails>();
                    foreach (var item in renewableAssessments)
                    {
                        DateTime renewalDate = new DateTime();
                        item.FrequencyValue = item.FrequencyValue == null ? 0 : item.FrequencyValue;
                        //if frequency value is 0, continue and pick another and log this one
                        if (item.FrequencyValue <= 0)
                        {
                            continue;
                        }

                        if (item.IsRenewableByDate == true)
                        {
                            renewalDate = item.AssessmentCreatedDate.AddYears((int)item.FrequencyValue);
                        }
                        else
                        {
                            renewalDate = new DateTime(item.AssessmentCreatedDate.Year + (int)item.FrequencyValue, 1, 1);
                        }

                        var detail = await _context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.BillDetailId && x.ServiceRevenueId == item.ServiceRevenueId);
                        if (detail != null)
                        {
                            detail.RenewalDate = renewalDate;
                            updateList.Add(detail);
                        }
                    }
                    _context.BillDetails.UpdateRange(updateList);
                    var save = await _context.SaveChangesAsync();

                    if (save > 0)
                    {
                        response.Message = "Sucessful";
                        response.Succeeded = true;
                    }
                     response.Message = "Unsucessful";
                }
                 response.Message = "No record to process";
                return response;
            }
            catch (Exception ex)
            {

                _logger.Error("Error on CalculateRenewalDate Method in PayValueRepository" + ex.InnerException == null ? ex.InnerException.Message : ex.InnerException.Message);
                response.Message = "Failed";
            }
            return response;
        }

        public async Task<Response<CreateAssessmentRequestDto>> CreateAssessment(CreateAssessmentRequestDto request)
        {
            var response = new Response<CreateAssessmentRequestDto>();
            using (var trans = _context.Database.BeginTransaction())
            {
                try
                {
                    var billInfo = _mapper.Map<BillInfo>(request);
                    billInfo.BillId = Guid.NewGuid();
                    var merchantRequireExternalPaymentCode = await _context.Agency.AnyAsync(x => x.Code == _authenticatedUser.AgencyCode && x.ExternalPaymentCodeRequired == true);
                    var baseNumber = GenerateBaseNumber(_appsettings.MerchantCode);

                    if (!merchantRequireExternalPaymentCode)
                    {
                        billInfo.PaymentCode = baseNumber;
                    }
                    billInfo.Address = string.IsNullOrWhiteSpace(billInfo.Address) ? "NOT PROVIDED" : billInfo.Address;
                    billInfo.BaseNumber = baseNumber;
                    billInfo.CreatedById = _authenticatedUser.UserId == null ? request.CreatedById : _authenticatedUser.UserId;
                    billInfo.AgencyCode = _authenticatedUser.AgencyCode == null ? request.AgencyCode : _authenticatedUser.AgencyCode;
                    billInfo.Telephone = string.IsNullOrEmpty(request.Telephone) ? "11111111111" : request.Telephone;
                    billInfo.Email = string.IsNullOrEmpty(request.Email) ? "payvalue@icmaservices.com" : request.Email;
                    billInfo.CreatedOn = DateTime.Now;
                    billInfo.TotalAssessed = request.BillDetails.Sum(obj => obj.BillAmount);
                    billInfo.Liability = request.BillDetails.Sum(obj => obj.Liability);
                    billInfo.ServiceName = request.BillDetails.FirstOrDefault().ServiceName;
                    billInfo.ServiceId = (long)request.BillDetails.First().ServiceId;
                    billInfo.TotalBillAmount = billInfo.TotalAssessed + billInfo.Liability;
                    var signatureId = await _context.AgencySignature.Select(x => new { x.Id, x.AgencyCode }).FirstOrDefaultAsync(x => x.AgencyCode == _authenticatedUser.AgencyCode);
                    if (signatureId != null)
                    {
                        billInfo.SignatureId = signatureId.Id;
                    }

                    var additionalBillInfoDetail = new List<BillAdditionalInfo>();
                    foreach (var adInfo in request.AdditionalBillInfo)
                    {
                        var obj = new BillAdditionalInfo
                        {
                            BillInfoId = billInfo.BillId,
                            FieldValue = adInfo.FieldValue,
                            AdditionalServiceDetailId = adInfo.AdditionalServiceDetailId
                        };

                        additionalBillInfoDetail.Add(obj);
                    }

                    await _context.BillAdditionalInfo.AddRangeAsync(additionalBillInfoDetail);
                    await _context.SaveChangesAsync();

                    int i = 0;
                    foreach (var item in billInfo.BillDetails)
                    {
                        if (!merchantRequireExternalPaymentCode)
                        {
                            var itemVirtualId = i + 1;
                            item.ItemPaymentCode = $"{baseNumber}{itemVirtualId}";
                        }
                        var rate = await _context.Rate.FirstOrDefaultAsync(x => x.Id == item.RateId);
                        if (item.ServiceRevenueId == null)
                        {
                            item.ServiceRevenueId = rate.ServiceRevenueId;
                        }
                        item.BillInfoGuid = billInfo.BillId;
                        item.ShowRebateAmount = true;
                        item.CreatedById = _authenticatedUser.UserId == null ? request.CreatedById : _authenticatedUser.UserId;
                        item.CreatedOn = DateTime.Now;
                        item.AgencyCode = _authenticatedUser.AgencyCode == null ? item.AgencyCode : _authenticatedUser.AgencyCode;
                        item.AgencyName = _authenticatedUser.AgencyName == null ? item.AgencyName : _authenticatedUser.AgencyName;
                        item.BillInfoId = billInfo.Id;
                        item.TotalBillAmount = item.BillAmount + (item.Liability == null ? 0 : item.Liability);

                        if (item.IsDepositRequired == true)
                        {
                            var depositConsent = new DepositOnConsent
                            {
                                AssessmentRefNumber = baseNumber,
                                CreatedBy = _authenticatedUser.UserId,
                                CreatedOn = DateTime.Now,
                                PaymentRefNumber = item.PaymentReferenceNum,
                                InUse = true,
                                PayerId = request.PayerUtin
                            };
                            await _context.DepositOnConsent.AddAsync(depositConsent);
                            await _context.SaveChangesAsync();
                        }

                        i++;
                    }

                    if (!HelpersClasses.IsEmailValid(billInfo.Email) || string.IsNullOrEmpty(billInfo.Email))
                    {
                        billInfo.Email = "payvalue@icmaservices.com";
                    }

                    if (!HelpersClasses.IsPhoneNumberValid(billInfo.Telephone) || string.IsNullOrEmpty(billInfo.Telephone))
                    {
                        billInfo.Email = "11111111111";
                    }

                    await _context.BillInfo.AddAsync(billInfo);
                    var saveBillInfo = await _context.SaveChangesAsync();
                    if (saveBillInfo > 0)
                    {
                        foreach (var savedDetail in billInfo.BillDetails)
                        {
                            if (savedDetail.ServiceRevenueId > 0 && IsRenewalRequired((long)savedDetail.ServiceRevenueId))
                            {
                                await CalculateRenewalDate(savedDetail.Id);
                            }
                        }

                        await _audit.CreateAudit(_authenticatedUser.UserId, $"Added a new assessment with Parameter of ${JsonConvert.SerializeObject(request)}");
                        trans.Commit();
                        response.Message = "Assessment created successfully";
                        response.Succeeded = true;
                        response.Data = request;
                        return response;
                    }

                    trans.Rollback();
                    response.Message = "The assessment was not created";
                    response.Succeeded = false;
                    response.Data = null;
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, "An error has occurred on CreateAssessment, PayValue Repository");
                    trans.Rollback();
                }
            }
            return response;
        }
    }

    }

