using AutoMapper;
using Dapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayValueManualSln.Application;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.DTOs.RateDto;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Infrastructure.Persistence.HangFireSerivces;
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
        private readonly IAccountService _accountService;
        private readonly IcmaCollectionContext _icmaContext;
        private readonly IHttpClientHelperService _genericHttpClientHandlerService;
        public Appsettings _appsettings { get; }

        public EntityMangerAsync(ApplicationDbContext context,IHttpClientHelperService clientHelperService, IAuditRepository audit, IOptions<Appsettings> appsettings, IDapper dapper, ILogger logger, IConfiguration config, IHttpClientHelperService httpClientHelperService, IMapper mapper, IAuthenticatedUserService authenticatedUserService, HttpClient httpClient, RateServices rateServices,IAccountService accountService, IcmaCollectionContext icmacontext)
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
            _accountService = accountService;
            _icmaContext = icmacontext;
            _httpClientHelperService = httpClientHelperService;
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
                var obj = new PayerDetails
                {
                    payerName = request.companyName ?? $"{request.firstName} {request.surname} {request.otherName}",
                    payerUtin = request.utin ?? request.payerUtin,
                    dateCreated = DateTime.Now,
                    email = request.contactEmail ?? request.email,
                    phoneNo = request.contactPhoneNo,
                    address = request.companyAddress ?? request.address,
                    IsApproved = null,
                    ChangeRequesterId = _authenticatedUser.UserId,
                    ApprovalComment = request.ApprovalComment
                };
                await _context.PayerDetails.AddAsync(obj);
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
                response.Data = list.Select(s => new AdditionalServiceDetailDto
                {
                    Id = s.Id,
                    AdditionalServiceDetailName = s.AdditionalServiceDetailName
                }).ToList();
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
        public async Task<Response<List<MapServiceToTypeRequestDto>>> MapServiceToType(List<MapServiceToTypeRequestDto> request)
        {
            var response = new Response<List<MapServiceToTypeRequestDto>>();
            try
            {
                var map = new List<MapServiceToTypeRequestDto>();
                foreach (var item in request)
                {
                    var typeName = await _context.Type.Select(x => new { x.Name, x.Id }).FirstOrDefaultAsync(x => x.Id == item.TypeId);
                    var obj = new MapServiceToTypeRequestDto
                    {
                        TypeId = item.TypeId,
                        ServiceId = item.ServiceId,
                        CreatedBy = _authenticatedUser.UserId ?? null,
                        TypeName = typeName.Name
                    };
                }
                await _context.AddRangeAsync(map);
                var save = await _context.SaveChangesAsync();
                response.Message = save > 0 ? "Successful" : "Unsuccessful";
                response.Succeeded = save > 0;
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "An error has occurred on MapServiceToType, PayValue Repository");

            }
            return response;
        }
        public async Task<Response<List<BillInfoDto>>> GetAllAssessmentsAsync(int? year, AssessmentStatus assessmentStatus = AssessmentStatus.All)
        {
            var response = new Response<List<BillInfoDto>>();
            year = year == null ? (int)DateTime.UtcNow.Year : year;
            var bills = new List<BillInfo>();
            if (assessmentStatus == AssessmentStatus.All)
            {
                bills = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsDeleted != true && x.CreatedOn.Year == year).ToListAsync();
            }
            if (assessmentStatus == AssessmentStatus.Approved)
            {
                bills = await _context.BillInfo
                    .Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsApproved == true && !string.IsNullOrEmpty(x.PaymentCode) && !string.IsNullOrEmpty(x.BaseNumber) && x.IsDeleted != true && !x.IsExpired && x.IsReversed != true && x.CreatedOn.Year == year)
                    .ToListAsync();
            }
            if (assessmentStatus == AssessmentStatus.Expired)
            {
                bills = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsExpired && x.CreatedOn.Year == year).ToListAsync();
            }
            if (assessmentStatus == AssessmentStatus.Disapproved)
            {
                bills = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsApproved == false && x.IsDeleted != true && x.CreatedOn.Year == year).ToListAsync();
            }
            if (assessmentStatus == AssessmentStatus.Pending)
            {
                bills = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsApproved == null && string.IsNullOrEmpty(x.PaymentCode) && !string.IsNullOrEmpty(x.BaseNumber) && x.IsDeleted != true && x.CreatedOn.Year == year).ToListAsync();
            }
            if (assessmentStatus == AssessmentStatus.PendingPayCode)
            {
                bills = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsApproved == true && string.IsNullOrEmpty(x.PaymentCode) && !string.IsNullOrEmpty(x.BaseNumber) && x.IsDeleted != true).ToListAsync();
            }

            try
            {
                var responses = new List<BillInfoDto>();
                if (bills.Count > 0)
                {
                    responses = bills.Select(x => new BillInfoDto
                    {
                        PayerName = x.PayerName,
                        CreatedOn = x.CreatedOn,
                        ApprovedOn = x.ApprovedOn,
                        PaymentCode = x.PaymentCode,
                        PayerUtin = x.PayerUtin,
                        BaseNumber = x.BaseNumber,
                        IsApproved = x.IsApproved,
                        AgencyCode = x.AgencyCode,
                        TotalBillAmount = x.TotalBillAmount,
                        BillId = x.BillId,
                        ServiceName = x.ServiceName,
                        CreatedById = x.CreatedById,
                        ServiceId = (long)x.ServiceId,
                        IsStandardLetterRequired = x.ServiceId != null
                            ? _context.Services.FirstOrDefault(s => s.Id == x.ServiceId)?.IsStandardLetterRequired ?? false
                            : false,
                        Id = x.Id,
                        CreatedBy = !string.IsNullOrEmpty(x.CreatedById)
                            ? _accountService.GetUserById(x.CreatedById)?.Data != null
                                ? $"{_accountService.GetUserById(x.CreatedById).Data.FirstName} {_accountService.GetUserById(x.CreatedById).Data.LastName}"
                                : null
                            : null,
                        ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy)
                        ? _accountService.GetUserById(x.ApprovedBy)?.Data != null
                        ? $"{_accountService.GetUserById(x.ApprovedBy).Data.FirstName} {_accountService.GetUserById(x.ApprovedBy).Data.LastName}"
                        : null
                        : null,
                    }).ToList();
                }
                response.Data = responses;
                response.Succeeded = true;
                response.Message = "Assessment retrieved successfully.";
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public async Task<Response<List<BillInfoDto>>> GetAssessmentPendingApprovalByUserIdAsync()
        {
            var response = new Response<List<BillInfoDto>>();
            var approvalSetting = await _context.ModuleApprovalConfig.FirstOrDefaultAsync(x => x.AgencyCode == _authenticatedUser.AgencyCode);
            if (approvalSetting != null)
            {
                if (approvalSetting.IsApprovalForGeneralValidators == false)
                {
                    var initiatorsMappedToUser = await _context.MapUserApproval.Where(x => x.ValidatorId == _authenticatedUser.UserId && x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower()).ToListAsync();
                    var userBills = new List<BillInfoDto>();
                    foreach (var item in initiatorsMappedToUser)
                    {
                        var records = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsApproved == null && x.CreatedById == item.InitiatorId)
                            .Include(x => x.BillDetails)
                            .Select(x => new BillInfoDto
                            {
                                ServiceName = x.BillDetails.FirstOrDefault().ServiceName,
                                TypeName = x.BillDetails.FirstOrDefault().TypeName,
                                Location = x.BillDetails.FirstOrDefault().LocationName,
                                Zone = x.BillDetails.FirstOrDefault().ZoneName,
                                PayerName = x.PayerName,
                                PayerUtin = x.PayerUtin,
                                TotalBillAmount = x.TotalBillAmount,
                                CreatedOn = x.CreatedOn,
                                Id = x.Id,
                                BillId = x.BillId,
                                TotalAssessed = x.TotalAssessed
                            }).ToListAsync();
                        userBills.AddRange(records);
                    }
                    response.Data = userBills;
                    response.Message = "Pending assessments retrieved successfully.";
                    response.Succeeded = true;
                    return response;
                }
            }
            var bills = await _context.BillInfo.Where(x => x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower() && x.IsApproved == null)
                            .Include(x => x.BillDetails)
                            .Select(x => new BillInfoDto
                            {
                                ServiceName = x.BillDetails.FirstOrDefault().ServiceName,
                                TypeName = x.BillDetails.FirstOrDefault().TypeName,
                                Location = x.BillDetails.FirstOrDefault().LocationName,
                                Zone = x.BillDetails.FirstOrDefault().ZoneName,
                                PayerName = x.PayerName,
                                PayerUtin = x.PayerUtin,
                                TotalBillAmount = x.TotalBillAmount,
                                CreatedOn = x.CreatedOn,
                                Id = x.Id,
                                BillId = x.BillId,
                                TotalAssessed = x.TotalAssessed
                            }).ToListAsync();
            response.Message = "Pending assessments retrieved successfully.";
            response.Succeeded = true;
            response.Data = bills;
            return response;
           }

        public async Task<Response<string>> ApproveAssessmentAsync(AssessmentApprovalRequest request)
        {
            var response = new Response<string>();
            try
            {
                var bill = await _context.BillInfo
                    .Include(x => x.BillDetails)
                    .FirstOrDefaultAsync(x => x.Id == request.AssessmentId && x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower());
                if (bill == null)
                {
                    response.Succeeded = false;
                    response.Message = "Record was not found";
                }
                else if (bill.IsApproved == true)
                {
                    response.Succeeded = false;
                    response.Message = "This assessment has already been approved";
                }
                else
                {
                    bill.IsApproved = request.IsApproved;
                    bill.DissaprovalComment = request.Comment;
                    bill.ApprovedBy = _authenticatedUser.UserId;
                    bill.ApprovedOn = DateTime.Now;

                    _context.BillInfo.Update(bill);
                    var update = await _context.SaveChangesAsync();
                    if (update > 0)
                    {
                        //Check if merchant required external payment code generation, if yes proceed to do below else skip
                        var merchantRequireExternalPaymentCode = await _context.Agency.AnyAsync(x => x.Code == bill.AgencyCode && x.ExternalPaymentCodeRequired == true);
                        if (merchantRequireExternalPaymentCode)
                        {
                            //Proceed to generate payment code from external provider as its required using hangfire job
                            BackgroundJob.Enqueue<ServiceScheduler>(x => x.GenerateExternalPaymentCodeAsync(bill.BaseNumber, false));
                        }
                        if (request.IsApproved)
                        {
                            foreach (var item in bill.BillDetails)
                            {
                                if (item.IsDepositRequired == true)
                                {
                                    var depositOnConsent = await _context.DepositOnConsents.FirstOrDefaultAsync(x => x.PaymentRefNumber == item.PaymentReferenceNum);
                                    if (depositOnConsent != null)
                                    {
                                        depositOnConsent.IsUsed = true;
                                        depositOnConsent.InUse = false;
                                        _context.DepositOnConsents.Update(depositOnConsent);
                                    }
                                    //DEBT: This needs to be accessed using the ICMA collction service
                                    var collectionRevenueInfo = await _icmaContext.CollectionReports.FirstOrDefaultAsync(c => c.PaymentRefNumber == item.PaymentReferenceNum && c.IsReversed == false);
                                    if (collectionRevenueInfo != null)
                                    {
                                        if (collectionRevenueInfo != null)
                                        {
                                            collectionRevenueInfo.AmountUsed = Math.Abs((decimal)item.Liability);
                                            collectionRevenueInfo.IsUsed = true;
                                            _icmaContext.CollectionReports.Update(collectionRevenueInfo);
                                        }
                                    }
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                        //Background job (Hangfire) is fired at this point to send to assessment repository
                        BackgroundJob.Enqueue<ServiceScheduler>(x => x.SendAssessmentToRepository(bill.BaseNumber));
                        response.Succeeded = true;
                        response.Message = "Assessment was approved successfully";
                    }
                    else
                    {
                        response.Succeeded = false;
                        response.Message = "Assessment was not approved";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                Log.Error(ex.Message, "An error has occured on ApproveAssessment, Payvalue Repository");
                response.Succeeded = false;
                response.Message = "An error occurred while processing your request";
            }
            return response;
        }
        public async Task<MessageClass> GenerateExternalPaymentCode(string baseNumber, bool? updateBill = false, CancellationToken cancellationToken = default)
        {
            var bc = new MessageClass();
            var response = new List<DemandNoticeDetails>();
            var billDetails = new List<BillDetails>();

            try
            {
                var assessResult = await _context.BillInfo.Where(x => x.BaseNumber == baseNumber && (x.IsAdditionalAssessmentRequired == false || x.IsAdditionalAssessmentRequired == null) && x.MergerRequestId == null && (x.IsDeleted == null || x.IsDeleted == false) && (x.IsReversed == false || x.IsReversed == null) && x.IsApproved == true)
               .ToListAsync(cancellationToken).ConfigureAwait(false);

                if (!assessResult.Any())
                {
                    bc.StatusId = -1;
                    bc.StatusMessage = $"No record found for Base Number {baseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                    return bc;
                }
                if (assessResult.Any())
                {
                    billDetails = await _context.BillDetails.Where(x => x.BillInfoGuid == assessResult.First().BillId).ToListAsync();
                    response.AddRange(billDetails.Select(invoiceDetails => new DemandNoticeDetails
                    { amount = (double)invoiceDetails.TotalBillAmount, name = invoiceDetails.RevenueName, revenue_code = invoiceDetails.RevenueCode }));

                }

                var assessDetailsResultFirstDefault = assessResult.FirstOrDefault();
                var bPMSPaymentCodeRequest = new BPMSPaymentCodeRequest();
                BMPSMessageClass serviceResponse = new BMPSMessageClass();
                if (updateBill == true)
                {
                    bPMSPaymentCodeRequest = new BPMSPaymentCodeRequest
                    {
                        customer_first_name = assessDetailsResultFirstDefault.PayerName,
                        customer_last_name = assessDetailsResultFirstDefault.PayerName,
                        customer_email = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Email) || !HelpersClasses.IsEmailValid(assessDetailsResultFirstDefault.Email) ? "payvalue@icmaservices.com" : assessDetailsResultFirstDefault.Email,
                        customer_phone = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Telephone) || !HelpersClasses.IsPhoneNumberValid(assessDetailsResultFirstDefault.Telephone) ? "11111111111" : assessDetailsResultFirstDefault.Telephone,
                        customer_address = string.IsNullOrWhiteSpace(assessDetailsResultFirstDefault.Address) ? "Not provided" : assessDetailsResultFirstDefault.Address,
                        bill_description = assessDetailsResultFirstDefault.ServiceName,
                        billed_amount = (double)response.Sum(x => x.amount),
                        overwrite_existing = true,
                        payment_code = assessDetailsResultFirstDefault.PaymentCode,
                        service_id = (int)_context.Services.Where(x => x.Id == assessDetailsResultFirstDefault.ServiceId).FirstOrDefault().ExternalServiceId,
                        request_id = baseNumber,
                        demand_notices = response
                    };
                    serviceResponse = await _genericHttpClientHandlerService.PostAsync<BPMSPaymentCodeRequest, BMPSMessageClass>($"{_appsettings.BPMSGateWayServiceUrl}{ApplicationConstants.UpdateBillingUrl}", bPMSPaymentCodeRequest);

                }
                else
                {
                    bPMSPaymentCodeRequest = new BPMSPaymentCodeRequest
                    {
                        customer_first_name = assessDetailsResultFirstDefault.PayerName,
                        customer_last_name = assessDetailsResultFirstDefault.PayerName,
                        customer_address = string.IsNullOrWhiteSpace(assessDetailsResultFirstDefault.Address) ? "Not provided" : assessDetailsResultFirstDefault.Address,
                        customer_email = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Email) || !HelpersClasses.IsEmailValid(assessDetailsResultFirstDefault.Email) ? "payvalue@icmaservices.com" : assessDetailsResultFirstDefault.Email,
                        customer_phone = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Telephone) || !HelpersClasses.IsPhoneNumberValid(assessDetailsResultFirstDefault.Telephone) ? "11111111111" : assessDetailsResultFirstDefault.Telephone,
                        bill_description = assessDetailsResultFirstDefault.ServiceName,
                        billed_amount = (double)response.Sum(x => x.amount),
                        overwrite_existing = false,
                        service_id = (int)_context.Services.Where(x => x.Id == assessDetailsResultFirstDefault.ServiceId).FirstOrDefault().ExternalServiceId,
                        request_id = baseNumber,
                        demand_notices = response
                    };
                    serviceResponse = await _genericHttpClientHandlerService.PostAsync<BPMSPaymentCodeRequest, BMPSMessageClass>($"{_appsettings.BPMSGateWayServiceUrl}{ApplicationConstants.CreateBillingUrl}", bPMSPaymentCodeRequest);

                }
                var msg = $"Record to be send to BPMS through GenerateExternalPaymentCode Method :- {JsonConvert.SerializeObject(bPMSPaymentCodeRequest)}";
                _logger.Information(msg);

                if (serviceResponse != null && serviceResponse.status == true && serviceResponse.status_code == 200)
                {
                    var validAssessment = assessResult.FirstOrDefault();
                    validAssessment.PaymentCode = serviceResponse.data.payment_code;
                    validAssessment.ExternalResponseDate = DateTime.Now;
                    validAssessment.ExternalResponseJson = JsonConvert.SerializeObject(serviceResponse);
                    _context.BillInfo.Update(validAssessment);
                    await _context.SaveChangesAsync();
                    //var updateList = new List<BillDetails>();
                    foreach (var item in serviceResponse.data.demand_notices)
                    {
                        var billDetailToUpdate = await _context.BillDetails.FirstOrDefaultAsync(x => x.RevenueCode == item.revenue_code);
                        if (billDetailToUpdate != null)
                        {
                            billDetailToUpdate.ItemPaymentCode = item.revenue_item_reference;
                            _context.BillDetails.Update(billDetailToUpdate);
                            await _context.SaveChangesAsync();

                        }
                    }
                    bc.StatusId = 1;
                    bc.StatusMessage = $"{serviceResponse.message} for Base No. {baseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                }
                if (serviceResponse != null && serviceResponse.status == false)
                {
                    bc.StatusId = -1;
                    bc.StatusMessage = $"{serviceResponse.message} for Base. No. {baseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                }
                if (serviceResponse == null)
                {
                    bc.StatusId = -1;
                    bc.StatusMessage = $"BPMS Service Return Null Value For Base. No. {baseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToString();
                bc.StatusId = -1;
                bc.StatusMessage = ex.Message;
                Log.Error(ex.InnerException == null ? $"Error occurred while processing your request at {nameof(GenerateExternalPaymentCode)}" : ex.InnerException.Message);
                return bc;
            }

            return bc;
        }
        public async Task<MessageClass> GenerateExternalPaymentCodeForPendingAssessment(CancellationToken cancellationToken = default)
        {
            var bc = new MessageClass();
            var response = new List<DemandNoticeDetails>();
            var assessDetailsResult = new List<BillInfo>();

            try
            {
                //
                var allAgencies = await _context.Agency.ToListAsync();
                if (allAgencies.Any())
                {
                    foreach (var item in allAgencies)
                    {
                        if (item.ExternalPaymentCodeRequired != true)
                        {
                            bc.StatusId = -1;
                            bc.StatusMessage = $"Agency {item.Code} does not require an external payment code generation";
                            _logger.Information(JsonConvert.SerializeObject(bc));
                            return bc;
                        }

                        var assessResult = await _context.BillInfo.Where(x => x.PaymentCode == null && x.AgencyCode == item.Code && x.IsAdditionalAssessmentRequired == false && x.MergerRequestId == null && (x.IsDeleted == null || x.IsDeleted == false) && (x.IsReversed == false || x.IsReversed == null) && x.IsApproved == true)
                 .ToListAsync(cancellationToken).ConfigureAwait(false);

                        if (!assessResult.Any())
                        {
                            bc.StatusId = -1;
                            bc.StatusMessage = $"No pending bill waiting for external payment code for this agency {item.Code}";
                            _logger.Information(JsonConvert.SerializeObject(bc));
                            return bc;
                        }
                        if (assessResult.Any())
                        {
                            var billDetails = await _context.BillDetails.Where(x => x.BillInfoGuid == assessResult.First().BillId).ToListAsync();
                            response.AddRange(billDetails.Select(invoiceDetails => new DemandNoticeDetails
                            { amount = (double)invoiceDetails.TotalBillAmount, name = invoiceDetails.RevenueName, revenue_code = invoiceDetails.RevenueCode }));

                        }

                        var assessDetailsResultFirstDefault = assessResult.FirstOrDefault();
                        var bPMSPaymentCodeRequest = new BPMSPaymentCodeRequest();

                        bPMSPaymentCodeRequest = new BPMSPaymentCodeRequest
                        {
                            customer_first_name = assessDetailsResultFirstDefault.PayerName,
                            customer_last_name = assessDetailsResultFirstDefault.PayerName,
                            customer_address = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Address) ? "Not provided" : assessDetailsResultFirstDefault.Address,
                            customer_email = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Email) || !HelpersClasses.IsEmailValid(assessDetailsResultFirstDefault.Email) ? "payvalue@icmaservices.com" : assessDetailsResultFirstDefault.Email,
                            customer_phone = string.IsNullOrEmpty(assessDetailsResultFirstDefault.Telephone) || !HelpersClasses.IsPhoneNumberValid(assessDetailsResultFirstDefault.Telephone) ? "11111111111" : assessDetailsResultFirstDefault.Telephone,
                            bill_description = assessDetailsResultFirstDefault.ServiceName,
                            billed_amount = (double)assessDetailsResultFirstDefault.TotalBillAmount,
                            overwrite_existing = false,
                            service_id = (int)_context.Services.Where(x => x.Id == assessDetailsResultFirstDefault.ServiceId).FirstOrDefault().ExternalServiceId,
                            request_id = assessResult.First().BaseNumber,
                            demand_notices = response
                        };

                        var msg = $"Record to be send to BPMS through GenerateExternalPaymentCode Method :- {JsonConvert.SerializeObject(bPMSPaymentCodeRequest)}";
                        _logger.Information(msg);

                        var serviceResponse = await _genericHttpClientHandlerService.PostAsync<BPMSPaymentCodeRequest, BMPSMessageClass>($"{_appsettings.BPMSGateWayServiceUrl}{ApplicationConstants.CreateBillingUrl}", bPMSPaymentCodeRequest);

                        if (serviceResponse != null && serviceResponse.status == true && serviceResponse.status_code == 200)
                        {
                            var validAssessment = assessDetailsResultFirstDefault;
                            validAssessment.PaymentCode = serviceResponse.data.payment_code;
                            validAssessment.ExternalResponseDate = DateTime.Now;
                            validAssessment.ExternalResponseJson = JsonConvert.SerializeObject(serviceResponse);
                            _context.BillInfo.Update(validAssessment);
                            await _context.SaveChangesAsync();
                            foreach (var notice in serviceResponse.data.demand_notices)
                            {
                                var billDetailToUpdate = await _context.BillDetails.FirstOrDefaultAsync(x => x.RevenueCode == notice.revenue_code);
                                if (billDetailToUpdate != null)
                                {
                                    billDetailToUpdate.ItemPaymentCode = notice.revenue_item_reference;
                                    _context.BillDetails.Update(billDetailToUpdate);
                                    await _context.SaveChangesAsync();

                                }
                            }
                            await _context.SaveChangesAsync();

                            bc.StatusId = 1;
                            bc.StatusMessage = $"{serviceResponse.message} for Base No. {assessResult.First().BaseNumber}";
                            _logger.Information(JsonConvert.SerializeObject(bc));
                        }
                        if (serviceResponse != null && serviceResponse.status == false)
                        {
                            bc.StatusId = -1;
                            bc.StatusMessage = $"{serviceResponse.message} for Base. No. {assessResult.First().BaseNumber}";
                            _logger.Information(JsonConvert.SerializeObject(bc));
                        }
                        if (serviceResponse == null)
                        {
                            bc.StatusId = -1;
                            bc.StatusMessage = $"BPMS Service Return Null Value For Base. No. {assessResult.First().BaseNumber}";
                            _logger.Information(JsonConvert.SerializeObject(bc));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToString();
                bc.StatusId = -1;
                bc.StatusMessage = ex.InnerException == null ? $"Error occurred while processing your request at {nameof(GenerateExternalPaymentCode)}" : ex.InnerException.Message;
                Log.Error(bc.StatusMessage);
                return bc;
            }

            return bc;
        }
        public async Task<MessageClass> WithDrawExternalPaymentCodeAsync(string formerBaseNumber, CancellationToken cancellationToken = default)
        {
            var bc = new MessageClass();
            BMPSMessageClass serviceResponse = new BMPSMessageClass();

            try
            {
                var assessResult = await _context.BillInfo.Where(x => x.BaseNumber == formerBaseNumber && (x.IsAdditionalAssessmentRequired == false || x.IsAdditionalAssessmentRequired == null) && x.MergerRequestId == null && x.IsDeleted == true && x.IsApproved == true && x.IsReversed == true)
               .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (assessResult == null)
                {
                    bc.StatusId = -1;
                    bc.StatusMessage = $"No record found for Base Number {formerBaseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                    return bc;
                }

                var request = new BPMSWithdrawBillRequest
                {
                    payment_code = assessResult.PaymentCode
                };

                serviceResponse = await _genericHttpClientHandlerService.PostAsync<BPMSWithdrawBillRequest, BMPSMessageClass>($"{_appsettings.BPMSGateWayServiceUrl}{ApplicationConstants.WithdrawBillingUrl}", request);

                var msg = $"Bill to be withdraw through WithDrawExternalPaymentCodeAsync Method :- {JsonConvert.SerializeObject(request)}";
                _logger.Debug(msg);

                if (serviceResponse != null && serviceResponse.status == true && serviceResponse.status_code == 200)
                {
                    assessResult.IsBillWithdrawn = serviceResponse.status;
                    assessResult.BillWithdrawnOn = DateTime.Now;
                    _context.BillInfo.Update(assessResult);
                    await _context.SaveChangesAsync();
                    bc.StatusId = 1;
                    bc.StatusMessage = $"{serviceResponse.message} for Base No. {formerBaseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                }
                if (serviceResponse != null && serviceResponse.status == false)
                {
                    bc.StatusId = -1;
                    bc.StatusMessage = $"{serviceResponse.message} for Base. No. {formerBaseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                }
                if (serviceResponse == null)
                {
                    bc.StatusId = -1;
                    bc.StatusMessage = $"BPMS Service Return Null Value For Base. No. {formerBaseNumber}";
                    _logger.Information(JsonConvert.SerializeObject(bc));
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToString();
                bc.StatusId = -1;
                bc.StatusMessage = ex.Message;
                Log.Error(ex.InnerException == null ? $"Error occurred while processing your request at {nameof(GenerateExternalPaymentCode)}" : ex.InnerException.Message);
                return bc;
            }

            return bc;
        }
        public async Task<Response<string>> SendToAssessmentRepository(string baseNumber = null)
        {
            var response = new Response<string>();
            try
            {
                var list = new List<BillInfo>();
                if (string.IsNullOrEmpty(baseNumber))
                {
                    list = await _context.BillInfo
                        .Where(x => x.IsSentToAssessmentRepository == false && x.IsApproved == true)
                        .Take(100)
                        .ToListAsync();
                }
                else
                {
                    list = await _context.BillInfo
                        .Where(x => x.IsSentToAssessmentRepository == false && x.BaseNumber == baseNumber && x.IsApproved == true)
                        .ToListAsync();
                }
                if (list.Count > 0)
                {
                    var merchantRequireExternalPaymentCode = await _context.Agency.AnyAsync(x => x.Code == _authenticatedUser.AgencyCode && x.ExternalPaymentCodeRequired == true);
                    if (!merchantRequireExternalPaymentCode)
                    {
                        foreach (var item in list)
                        {
                            var billDetails = await _context.BillDetails
                                .Where(b => b.BillInfoGuid == item.BillId)
                                .ToListAsync();

                            var invoiceDetail = billDetails
                                .Select(x => new InvoiceDetails
                                {
                                    RevenueCode = x.RevenueCode,
                                    ItemAmount = (decimal)x.BillAmount,
                                    ItemArrears = x.Liability == null ? 0 : x.Liability,
                                    Narration = x.ServiceName,
                                    ItemPaymentCode = x.ItemPaymentCode,
                                    ItemTransactionId = x.BaseNumberItemRefNo
                                }).ToList();

                            var location = !string.IsNullOrEmpty(billDetails.FirstOrDefault().LocationName) ? billDetails.FirstOrDefault().LocationName : _appsettings.MerchantCode;
                            var repoRequest = new GenerateInvoiceRequest
                            {
                                TransactionId = item.BaseNumber,
                                PayerId = item.PayerUtin,
                                PayerName = item.PayerName,
                                PayerEmail = item.Email,
                                Telephone = item.Telephone,
                                PaymentPeriod = item.CreatedOn.Year.ToString(),
                                Address = item.Address,
                                TotalAmount = (decimal)billDetails.Sum(x => x.TotalBillAmount),
                                Arrears = 0M,
                                Narration = $"Online Direct Assessment Invoice",
                                Location = location,
                                InvoiceYear = item.CreatedOn.Year.ToString(),
                                PlatformCode = _appsettings.PlatformCode,
                                RaisedBy = $"{item.PayerName} <{item.Email}>",
                                RaisedOn = item.CreatedOn,
                                ApprovedBy = item?.ApprovedBy != null ? $"{_accountService.GetUserById(item.ApprovedBy)?.Data?.FirstName} {_accountService.GetUserById(item.ApprovedBy)?.Data?.LastName}" : "Agency Admin",
                                ApprovedOn = item.ApprovedOn == null ? item.CreatedOn : item.ApprovedOn,
                                UpdatePreviousInvoice = false,
                                ItemCount = billDetails.Count,
                                AsExpiryDate = false,
                                PreviousYearAssessmentRefNo = null,
                                RevenueCode = _appsettings.UnclassifiedRevenueCode,
                                InvoiceDetails = invoiceDetail,
                                PaymentCode = item.PaymentCode
                            };
                            var json = JsonConvert.SerializeObject(repoRequest);
                            var responses = await _genericHttpClientHandlerService.PostAsync<GenerateInvoiceRequest, Response<ExternalApiResponseDto<SendAssessmentToRepositoryResponseData>>>($"{_appsettings.AssessmentRepoServiceBaseUrl}{_appsettings.GenerateInvoiceNoMethod}", repoRequest);

                            if (response != null && response.Succeeded)
                            {
                                item.IsSentToAssessmentRepository = true;

                                _context.BillInfo.Update(item);
                                await _context.SaveChangesAsync();
                            }
                        }

                        return ApplicationConstants.SuccessMessage("Update was successful");
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            var billDetails = await _context.BillDetails
                                .Where(b => b.BillInfoGuid == item.BillId)
                                .ToListAsync();

                            var invoiceDetail2 = billDetails
                                .Select(x => new InvoiceDetails
                                {
                                    RevenueCode = x.RevenueCode,
                                    ItemAmount = (decimal)x.TotalBillAmount,
                                    ItemArrears = (x.TotalBillAmount) - (x.BillAmountPaid),
                                    Narration = $"Payment for {x.RevenueName}",

                                }).ToList();

                            var location = !string.IsNullOrEmpty(billDetails.FirstOrDefault().LocationName) ? billDetails.FirstOrDefault().LocationName : _appsettings.MerchantCode;

                            var repoRequest = new GenerateInvoiceRequest
                            {
                                TransactionId = item.BaseNumber,
                                PayerId = item.PayerUtin,
                                PayerName = item.PayerName,
                                PayerEmail = item.Email,
                                Telephone = item.Telephone,
                                PaymentPeriod = item.CreatedOn.Year.ToString(),
                                Address = item.Address,
                                TotalAmount = (decimal)billDetails.Sum(x => x.TotalBillAmount),
                                Arrears = 0M,
                                Narration = $"Payment for {item.ServiceName}",
                                Location = location,
                                InvoiceYear = item.CreatedOn.Year.ToString(),
                                PlatformCode = _appsettings.PlatformCode,
                                RaisedBy = $"{item.PayerName} <{item.Email}>",
                                RaisedOn = item.CreatedOn,
                                ApprovedBy = item.ApprovedBy != null ? $"{_accountService.GetUserById(item.ApprovedBy).Data.FirstName} {_accountService.GetUserById(item.ApprovedBy).Data.LastName}" : "Agency Admin",
                                ApprovedOn = item.ApprovedOn == null ? item.CreatedOn : item.ApprovedOn,
                                UpdatePreviousInvoice = false,
                                ItemCount = billDetails.Count,
                                AsExpiryDate = false,
                                PreviousYearAssessmentRefNo = null,
                                InvoiceDetails = invoiceDetail2,
                                RevenueCode = _appsettings.UnclassifiedRevenueCode,
                                PaymentCode = item.PaymentCode,
                            };

                            var responses = await _genericHttpClientHandlerService.PostAsync<GenerateInvoiceRequest, Response<ExternalApiResponseDto<SendAssessmentToRepositoryResponseData>>>($"{_appsettings.AssessmentRepoServiceBaseUrl}{_appsettings.GenerateInvoiceNoMethod}", repoRequest);

                            if (response != null && response.Succeeded)
                            {
                                item.IsSentToAssessmentRepository = true;
                                item.PaymentCode = responses.Data.Data.InvoiceNo;

                                if (string.IsNullOrEmpty(item.BaseNumber))
                                {
                                    item.BaseNumber = responses.Data.Data.TransactionId;
                                    var counter = 1;
                                    var listOfDetail = new List<BillDetails>();
                                    foreach (var det in billDetails)
                                    {
                                        det.BaseNumberItemRefNo = $"{baseNumber}{counter}";
                                        counter++;
                                        listOfDetail.Add(det);
                                    }
                                    if (listOfDetail.Count > 0)
                                    {
                                        _context.BillDetails.UpdateRange(listOfDetail);
                                        await _context.SaveChangesAsync();
                                    }
                                }

                                _context.BillInfo.Update(item);
                                await _context.SaveChangesAsync();
                            }
                        }

                        return ApplicationConstants.SuccessMessage("Update was successful");
                    }

                }

                return ApplicationConstants.NotFoundMessage("No record found");
            }
            catch (Exception ex)
            {
                Log.Error("Error on SendToAssessmentRepository Method in PayValueRepository" + ex.InnerException == null ? ex.InnerException.Message : ex.InnerException.Message);
                response.Message = "The Request Failed";
            }
            return response;
        }
        public async Task<Response<string>> RenewAssessments(long? billDetailId = null, bool? isManualRenewal = false)
        {
            var response = new Response<string>();
            try
            {
                var renewableAssessments = new List<AssessmentRenewalDto>();
                var currentYear = DateTime.Now.Year;

                var query = from bd in _context.BillDetails
                            join sr in _context.ServiceRevenue on bd.ServiceRevenueId equals sr.Id
                            where bd.RenewalDate.HasValue
                                  && bd.RenewalDate.Value.Year == currentYear
                                  && (bd.IsRenewed == false || bd.IsRenewed == null)
                                  && (bd.IsReversed == false || bd.IsReversed == null)
                            select new AssessmentRenewalDto
                            {
                                IsRenewable = sr.IsRenewable,
                                AllowAutomaticTrigger = sr.AllowAutomaticTrigger,
                                AutomaticApproval = sr.AutomaticApproval,
                                RenewalFrequencyId = sr.RenewalFrequencyId,
                                IsRenewableByDate = sr.IsRenewableByDate,
                                BillDetailId = bd.Id,
                                AssessmentCreatedDate = bd.CreatedOn,
                                ServiceRevenueId = sr.Id,
                                RenewalDate = bd.RenewalDate
                            };

                if (billDetailId != null)
                {
                    query = query.Where(x => x.BillDetailId == billDetailId);
                }
                else
                {
                    query = query.Where(x => x.AllowAutomaticTrigger == !isManualRenewal);
                }

                renewableAssessments = await query.AsNoTracking().ToListAsync();


                if (renewableAssessments.Count > 0)
                {
                    var billDetialUpdateList = new List<BillDetails>();
                    foreach (var item in renewableAssessments)
                    {
                        var serviceRevenue = await _context.ServiceRevenue.FirstOrDefaultAsync(x => x.Id == item.ServiceRevenueId);

                        if ((serviceRevenue.IsRenewableByDate == true && item.RenewalDate == DateTime.UtcNow.Date) || ((serviceRevenue.IsRenewableByDate == null || serviceRevenue.IsRenewableByDate == false) && item.RenewalDate.Value.Year == currentYear))
                        {
                            var assessmetBillDetail = await _context.BillDetails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.BillDetailId);
                            if (assessmetBillDetail != null)
                            {
                                var billInfo = await _context.BillInfo.AsNoTracking().Where(x => x.BillId == assessmetBillDetail.BillInfoGuid && (x.IsRenewed == null || x.IsRenewed == false)).FirstOrDefaultAsync();
                                if (billInfo != null)
                                {
                                    var billDetailsToRenew = await _context.BillDetails.AsNoTracking().Where(x => x.BillInfoGuid == billInfo.BillId).ToListAsync();
                                    var assesmmentToCreate = _mapper.Map<CreateAssessmentRequestDto>(billInfo);
                                    var detailList = new List<BillDetails>();

                                    assesmmentToCreate.PreviousBaseNumber = billInfo.BaseNumber;
                                    assesmmentToCreate.PreviousPaymentCode = billInfo.PaymentCode;
                                    if (serviceRevenue.AutomaticApproval == true)
                                    {
                                        assesmmentToCreate.IsApproved = true;
                                    }
                                    else
                                    {
                                        assesmmentToCreate.IsApproved = null;
                                    }
                                    foreach (var renewalDetail in billDetailsToRenew)
                                    {
                                        renewalDetail.Liability = renewalDetail.BillBalance == null ? 0 : renewalDetail.BillBalance;
                                        renewalDetail.TotalBillAmount = (renewalDetail.Liability == null ? 0 : renewalDetail.Liability) + (renewalDetail.BillBalance == null ? 0 : renewalDetail.BillBalance);
                                        renewalDetail.CreatedById = renewalDetail.CreatedById;
                                        renewalDetail.PreviousItemPaymentCode = renewalDetail.ItemPaymentCode;
                                        renewalDetail.PreviousBaseNumber = renewalDetail.BaseNumberItemRefNo;
                                        detailList.Add(renewalDetail);
                                    }   

                                    var additionalBillDetails = await _context.BillAdditionalInfo.Where(x => x.BillInfoId == billInfo.BillId)
                                        .Select(x => new AdditionalBillRequestDto
                                        {
                                            AdditionalServiceDetailId = x.AdditionalServiceDetailId,
                                            FieldValue = x.FieldValue
                                        }).ToListAsync();
                                    assesmmentToCreate.AdditionalBillInfo = additionalBillDetails;
                                    assesmmentToCreate.AgencyCode = billInfo.AgencyCode;
                                    assesmmentToCreate.CreatedById = billInfo.CreatedById;
                                    assesmmentToCreate.BillDetails = _mapper.Map<List<BillDetail>>(detailList);
                                    var renew = await CreateAssessment(assesmmentToCreate);
                                    if (renew.Succeeded)
                                    {
                                        var billInfoToRenew = await _context.BillInfo.FirstOrDefaultAsync(x => x.Id == billInfo.Id);
                                        var newlySavedBillDetails = new List<BillDetails>();
                                        if (billInfoToRenew != null)
                                        {
                                            newlySavedBillDetails = await _context.BillDetails.Where(x => x.BillInfoGuid == billInfoToRenew.BillId).ToListAsync();
                                            foreach (var detail in newlySavedBillDetails)
                                            {
                                                detail.IsRenewed = true;
                                                billDetialUpdateList.Add(detail);
                                            }

                                            billInfoToRenew.IsRenewed = true;
                                            _context.Update(billInfoToRenew);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (billDetialUpdateList.Count > 0)
                    {
                        _context.BillDetails.UpdateRange(billDetialUpdateList);
                        var save = await _context.SaveChangesAsync();
                        if (save > 0)
                        {
                            response.Message = "Success";
                        }
                           
                    }
                    response.Message = "Unsuccessful";
                }
                response.Message = "No record to process";
            }
            catch (Exception ex)
            {

                Log.Error("Error on RenewAssessments Method in PayValueRepository" + ex.InnerException == null ? ex.InnerException.Message : ex.InnerException.Message);
                response.Message = "Failed";
            }
            return response;
        }
    }
    }

        

