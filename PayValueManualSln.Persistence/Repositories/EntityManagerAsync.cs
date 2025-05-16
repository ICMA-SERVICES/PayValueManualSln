using AutoMapper;
using Dapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayValueManualSln.Application;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.DTOs.External;
using PayValueManualSln.Application.DTOs.RateDto;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Domain.Common;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Infrastructure.Persistence.HangFireSerivces;
using PayValueManualSln.Infrastructure.Persistence.Helpers;
using PayValueManualSln.Persistence.Helpers;
using PayValueManualSln.Persistence.Services;
using PayValueManualSln.Shared.DapperServices;
using PayValueV2.Domain.Entities.PayValue;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly IMemoryCache _cache;
        public Appsettings _appsettings { get; }

        public EntityMangerAsync(ApplicationDbContext context, IMemoryCache cache, IHttpClientHelperService clientHelperService, IAuditRepository audit, IOptions<Appsettings> appsettings, IDapper dapper, ILogger logger, IConfiguration config, IHttpClientHelperService httpClientHelperService, IMapper mapper, IAuthenticatedUserService authenticatedUserService, HttpClient httpClient, RateServices rateServices, IAccountService accountService, IcmaCollectionContext icmacontext)
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
            _cache = cache;
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
                    ApprovalComment = request.ApprovalComment,
                    contactName = request.contactName,
                    cacNumber = request.cacRegNumber,
                    address1 = request.companyAddress,
                    contactPersonEmail=request.contactEmail,
                    employeeName = request.firstName,
                    courtesyTitle = request.courtesyTitle,
                    businessTypeId = request.businessTypeId,
                    jtbTin = request.jtbtin,
                    revenueOfficeID = request.revenueOfficeID,
                    townID = request.townId,
                    payerType = request.payerType,
                    payerCategory = request.payerCategory,
                    lgaId = request.lgaId,
                    firstName = request.firstName,
                    otherName = request.otherName,
                    utin = request.utin
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
                    response.Data = result;
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
        public async Task<List<AssessmentDTO>> GetAllAssessmentDetails(string decodedPaymentCodeString)
        {
            var sources = new List<AssessmentDTO>();

            var checkForMergeAssessment = await _context.BillInfo.Where(x => x.PaymentCode == decodedPaymentCodeString && x.IsAdditionalAssessmentRequired == true && x.IsPrimaryAssessment == true && x.MergerRequestId != null && x.IsDeleted == false).ToListAsync().ConfigureAwait(false);

            if (checkForMergeAssessment.Any())
            {
                var agencyResult = await _context.Agency.Where(x => x.Code == checkForMergeAssessment.First().AgencyCode).FirstAsync();
                var assDetail = await (from a in _context.BillInfo
                                       join f in _context.BillDetails
                                       on a.BillId equals f.BillInfoGuid
                                       join b in _context.Services
                                         on a.ServiceId equals b.Id
                                       join c in _context.AgencySignature on a.AgencyCode equals c.AgencyCode
                                       join e in _context.Agency on a.AgencyCode equals e.Code
                                       join d in _context.Types
                                       on a.BillDetails.First().TypeId equals d.Id
                                       where a.IsReversed != true && a.IsDeleted != true && a.IsApproved == true && b.IsStandardLetterRequired == true && a.PaymentCode == decodedPaymentCodeString && c.IsActive == true

                                       select new AssessmentDTO
                                       {
                                           SignatoryName = b.SignatoryName,
                                           SignatoryPosition = b.SignatoryPosition,
                                           RentRevisionPeriod = c.RentRevisionPeriod,
                                           UsedCategoryName = d.Name,
                                           AssessmentCategoryName = d.Name,
                                           RequestId = a.BillId.ToString(),
                                           Name = a.PayerName,
                                           PaymentCode = a.PaymentCode,
                                           RevenueName = a.BillDetails.First().RevenueName,
                                           PartPaymentAllow = a.BillDetails.First().PartPaymentAllow,
                                           IsMailSent = a.IsMailSent,
                                           AgencyEmail = agencyResult.Email,
                                           AgencyPhone = agencyResult.Phone,
                                           AgencyName = agencyResult.Name,
                                           AgencyAddress1 = agencyResult.OfficialAddress1,
                                           AgencyAddress2 = agencyResult.OfficialAddress2,
                                           AgencyLogo = agencyResult.AgencyLogo.Image,
                                           AgencySignature = agencyResult.AgencySignature.Image,
                                           PayerRefNo = a.PayerUtin,
                                           Address = a.Address,
                                           BillInfoGuid = a.BillId,
                                           Telephone = a.Telephone,
                                           StateGovernorName = e.GovernorName,
                                           ServiceHeader = b.ServiceHeader,
                                           ServiceSubHeader = b.ServiceSubHeader,
                                           StandardLetterDescriptions = b.StandardLetterDescriptions,
                                           Email = a.Email,
                                           Location = _appsettings.StateName,
                                           //DateSubmitted = Convert.ToDateTime(a.DateSubmitted.Value.ToString("dd MMM yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo)),
                                           DateSubmitted = a.CreatedOn,
                                           ServiceId = b.Id,
                                           ServiceName = b.Name,
                                           PaymentItem = a.BillDetails.First().PaymentItemName,
                                           BaseAssnumber = a.PaymentCode,
                                           AssessementRefNo = a.PaymentCode,
                                           AssessementPeriod = a.BillPeriod,
                                           RebateAmount = a.BillDetails.First().RebateAmount,
                                           AssesementAmount = a.TotalAssessed,
                                           AssessmentBalance = a.BillDetails.First().BillBalance,
                                           RebatePercentage = Convert.ToInt32(a.BillDetails.First().RebatePercentage),
                                           ShowRebatePercentage = a.BillDetails.First().ShowRebateAmount,
                                           AssesementAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                           RebateAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().RebateAmount).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                           AssessmentBalanceFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().BillBalance).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                           TotalAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed) - Convert.ToDecimal(a.BillDetails.First().RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                           AssesementAmountpaid = a.BillDetails.First().BillAmountPaid
                                       }).ToListAsync().ConfigureAwait(false);

                sources = assDetail;
                return sources;
            }

            var assDetails = await (from a in _context.BillInfo
                                    join f in _context.BillDetails
                                    on a.BillId equals f.BillInfoGuid
                                    join b in _context.Services
                                    on a.ServiceId equals b.Id
                                    join c in _context.AgencySignature
                                    on a.AgencyCode equals c.AgencyCode
                                    join d in _context.Types
                                    on a.BillDetails.First().TypeId equals d.Id
                                    join e in _context.Agency
                                      on a.AgencyCode equals e.Code
                                    where a.IsDeleted != true && a.IsApproved == true && a.IsReversed != true
                                    && b.IsStandardLetterRequired == true && a.BaseNumber == decodedPaymentCodeString && c.IsActive == true
                                    select new AssessmentDTO
                                    {
                                        SignatoryName = b.SignatoryName,
                                        SignatoryPosition = b.SignatoryPosition,
                                        RentRevisionPeriod = c.RentRevisionPeriod,
                                        UsedCategoryName = d.Name,
                                        AssessmentCategoryName = d.Name,
                                        RequestId = a.BillId.ToString(),
                                        Name = a.PayerName,
                                        PaymentCode = a.PaymentCode,
                                        RevenueName = a.BillDetails.First().RevenueName,
                                        PartPaymentAllow = a.BillDetails.First().PartPaymentAllow,
                                        IsMailSent = a.IsMailSent,
                                        AgencyEmail = e.Email,
                                        AgencyPhone = e.Phone,
                                        AgencyName = e.Name,
                                        AgencyAddress1 = e.OfficialAddress1,
                                        AgencyAddress2 = e.OfficialAddress2,
                                        AgencyLogo = e.AgencyLogo.Image,
                                        AgencySignature = e.AgencySignature.Image,
                                        PayerRefNo = a.PayerUtin,
                                        //$"{a.Lastname} {a.Firstname} {a.Othernames}",
                                        Address = a.Address,
                                        Telephone = a.Telephone,
                                        BillInfoGuid = a.BillId,
                                        StateGovernorName = e.GovernorName,
                                        ServiceHeader = b.ServiceHeader,
                                        ServiceSubHeader = b.ServiceSubHeader,
                                        StandardLetterDescriptions = b.StandardLetterDescriptions,
                                        Email = a.Email,
                                        Location = _appsettings.StateName,
                                        //DateSubmitted = Convert.ToDateTime(a.DateSubmitted.Value.ToString("dd MMM yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo)),
                                        DateSubmitted = a.CreatedOn,
                                        ServiceId = b.Id,
                                        ServiceName = b.Name,
                                        PaymentItem = a.BillDetails.First().PaymentItemName,
                                        BaseAssnumber = a.PaymentCode,
                                        AssessementRefNo = a.PaymentCode,
                                        AssessementPeriod = a.BillPeriod,
                                        RebateAmount = a.BillDetails.First().RebateAmount,
                                        AssesementAmount = a.TotalAssessed,
                                        AssessmentBalance = a.BillDetails.First().BillBalance,
                                        RebatePercentage = Convert.ToInt32(a.BillDetails.First().RebatePercentage),
                                        ShowRebatePercentage = a.BillDetails.First().ShowRebateAmount,
                                        AssesementAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                        RebateAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().RebateAmount).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                        AssessmentBalanceFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().BillBalance).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                        TotalAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed) - Convert.ToDecimal(a.BillDetails.First().RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                        AssesementAmountpaid = a.BillDetails.First().BillAmountPaid
                                    }).ToListAsync().ConfigureAwait(false);

            sources = assDetails;

            return sources;

        }
        public string GetGovernorConsentHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var payerFileNo = string.Empty;
            var sizeOfLand = string.Empty;
            var termsofGrant = string.Empty;
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            //var payerFileNo = "ODD/PL/C.30/94";
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower()); //TitleCase
            var payerstate = assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "WITHOUT PREJUDICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var secondSignatoryPosition = "For: Special Adviser/Director General (Lands)";
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = assessmentDetailFirstDefault.ServiceHeader;
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var agencyName = assessmentDetailFirstDefault.AgencyName.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var agencyLogo = assessmentDetailFirstDefault.AgencyLogo;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            var offerShortName = "balance of &nbsp;Consent";

            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
                assigneeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assigneeName.ToLower());
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }


            var consentType = "INTEREST";  //ask Arotutu for solution on this

            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div class='thirdheader' style='text-align: justify;'><br /> 2. You are requested to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable on the {offerShortName} as evidence of acceptance of this offer. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS.</div>";
            //&nbsp;&nbsp;&nbsp;&nbsp;
            var LandText = $"3. Kindly notify the Bureau of your full payment of consent fee for further processing of your title document. <br /><br /> 4. Your prompt payment of all fee/charges into the coffers of Ogun State Government will hasten further processing of your application for Governor's consent to Deed of Assignment. <br /><br /> 5. Many thanks.";

            var standardLetterDescriptions = assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} {consentType} IN LAND SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()}. {payerstate}.";

            var firstBodyWord = $"<div><b>ASSIGNEE : &nbsp;{assigneeName}.</b></div> <br>Refer to the above subject matter, please. <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b> &nbsp;has graciously considered your request and approved the {serviceSubHeader}";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);
            //<body style='width: 97%; padding-top: 30%;'>

            sb.AppendFormat(@$"<html>
                            <head>
                            </head>
                            <body style='width: 97%'><br><br><div style='text-align: left;' class='flex-container'>{payerFileNo}</div>");

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div>");



            if (!string.IsNullOrEmpty(salutationName))
            {
                sb.Append(@$"{salutationName} <br>");
            }

            if (!string.IsNullOrEmpty(assigneeName))
            {
                sb.Append(@$"{assigneeName} <br>");
                sb.Append(@"C/o");
            }

            sb.AppendFormat(@"
                        <div class='row'>
                            <div class='column'> {0}<br> {1}</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{2} <br> Phone No.: {3}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{4}</q> <br> <span class='secondheader'>{5}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{6}</div> <br>
                            <div class='row'>
                            <div class='column'> <b>Terms</b> <br> Size:  <br> <br> <b>FEES</b></div>
                            <div class='column'>&nbsp;<br> {7} </div>
                        </div>"
                            , salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords, sizeOfLand);

            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();

                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }


            sb.Append(@"</table>");

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);

            sb.AppendFormat(@"{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/>", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<br><br><br><br><br><br><div style='text-align: center; padding-top: 10px;'>_____________________________</div>
                                <p style='text-align: center;'> {0} <br> {1} <br> {2}</p>
                             </body>
                            </html>
                            ", signatoryName, signatoryPosition, secondSignatoryPosition);


            return sb.ToString();
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
                    var billInfo = new BillInfo
                    {
                        PayerName = request.PayerName,
                        PayerUtin = request.PayerUtin,
                        Email = request.Email,
                        Address = request.Address,
                        Telephone = request.Telephone,
                        PaymentCode = request.PaymentCode,
                        IsAdditionalAssessmentRequired = request.IsAdditionalAssessmentRequired,
                        IsPrimaryAssessment = request.IsPrimaryAssessment,
                        BillPeriod = request.BillPeriod,
                        SignatureId = request.SignatureId,
                        IsMailSent = request.IsMailSent,
                        IsMailSendingRequired = request.IsMailSendingRequired,
                        IsRebated = request.IsRebated,
                        ClientId = request.ClientId,
                        ServiceId = request.ServiceId,
                        Pages = request.Pages,
                        Value = request.Value,
                        LandSize = request.LandSize,
                        AgencyCode = request.AgencyCode,
                        CreatedById = request.CreatedById,
                        IsApproved = request.IsApproved,
                        CreatedBy = _authenticatedUser.UserId
                    };

                    billInfo.BillId = Guid.NewGuid();
                    var merchantRequireExternalPaymentCode = await _context.Agency.AnyAsync(x => x.Code == _authenticatedUser.AgencyCode && x.ExternalPaymentCodeRequired == true);
                    var baseNumber = GenerateBaseNumber(_appsettings.MerchantCode);

                    if (!merchantRequireExternalPaymentCode)
                    {
                        billInfo.PaymentCode = baseNumber;
                    }
                    billInfo.BillDetails = request.BillDetails?.Select(item => new BillDetails
                    {
                        RateId = item.RateId,
                        ServiceId = item.ServiceId,
                        ServiceRevenueId = item.ServiceRevenueId,
                        ServiceName = item.ServiceName,
                        TypeId = item.TypeId,
                        TypeName = item.TypeName,
                        ZoneId = item.ZoneId,
                        ZoneName = item.ZoneName,
                        LocationId = item.LocationId,
                        LocationName = item.LocationName,
                        PaymentItemName = item.PaymentItemName,
                        BillAmount = item.BillAmount,
                        Liability = item.Liability,
                        RevenueCode = item.RevenueCode,
                        RevenueName = item.RevenueName,
                        PaymentReferenceNum = item.PaymentReferenceNum,
                        IsDepositRequired = item.IsDepositRequired,
                        AgencyCode = item.AgencyCode,
                    }).ToList();

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
                    billInfo.CreatedBy = _authenticatedUser.UserId;
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
                            AdditionalServiceDetailId = adInfo.AdditionalServiceDetailId,
                            CreatedBy = _authenticatedUser.UserId,
                            CreatedOn = DateTime.Now,
                           
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
                    var typeName = await _context.Types.Select(x => new { x.Name, x.Id }).FirstOrDefaultAsync(x => x.Id == item.TypeId);
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
        public async Task<Response<BillInfoDto>> GetAssessmentsByIdAsync(long billInfoId)
        {
            var response = new Response<BillInfoDto>();
            var bill = await _context.BillInfo.Where(x => x.Id == billInfoId && x.AgencyCode.TrimEnd().ToLower() == _authenticatedUser.AgencyCode.TrimEnd().ToLower())
                .Include(x => x.BillDetails)
                .FirstOrDefaultAsync();
            var request = _mapper.Map<BillInfoDto>(bill);
            var user = _accountService.GetUserById(request.CreatedById);
            request.CreatedBy = $"{user.Data.FirstName} {user.Data.LastName}";
            request.TypeName = bill.BillDetails.FirstOrDefault().TypeName;
            request.Location = bill.BillDetails.FirstOrDefault().LocationName;
            request.Zone = bill.BillDetails.FirstOrDefault().ZoneName;
            response.Message = "Assessment retrieved successfully";
            response.Succeeded = true;
            response.StatusCode = 200;
            return response;
        }
        public async Task<Response<string>> UpdateBaseNumber(long billId, string baseNumber)
        { var response = new Response<string>();
            var bill = await _context.BillInfo
                .Include(x => x.BillDetails)
                .FirstOrDefaultAsync(x => x.Id == billId);
            if (bill != null)
            {
                bill.BaseNumber = baseNumber;
                _context.BillInfo.Update(bill);
                var save = await _context.SaveChangesAsync();
                if (save > 0)
                {
                    var counter = 1;
                    var listOfDetail = new List<BillDetails>();
                    foreach (var det in bill.BillDetails)
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
               response.Message = "Base Number was updated successfully";
                response.Succeeded = true;
                response.StatusCode = 200;
            }
            return response;
    }
        public async Task<Response<List<GetDepositResponseDto>>> GetDepositAmount(List<GetDepositRequestDto> request)
        {
            var response = new List<GetDepositResponseDto>();
            //var bc = new MessageClass();
            foreach (var item in request)
            {
                var isExistInDepositTable = await _context.DepositOnConsents.FirstOrDefaultAsync(x => x.PaymentRefNumber == item.PaymentRefNumber);
                if (isExistInDepositTable != null && isExistInDepositTable.InUse == true)
                {
                    response.Add(
                        new GetDepositResponseDto
                        {
                            PaymentReferenceNumber = item.PaymentRefNumber,
                            Error = "This payment ref no. is in-review"
                        }
                    );
                }
                if (isExistInDepositTable != null && isExistInDepositTable.IsUsed == true)
                {
                    response.Add(
                      new GetDepositResponseDto
                      {
                          PaymentReferenceNumber = item.PaymentRefNumber,
                          Error = "This payment ref no. has been used"
                      }
                  );
                }
                var collectionRevenueInfo = await _icmaContext.CollectionReports.FirstOrDefaultAsync(c => c.PaymentRefNumber == item.PaymentRefNumber && c.IsReversed == false);
                if (collectionRevenueInfo is null)
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "No record found for payment ref number specified"
                     }
                 );
                }
                else if (!string.Equals(collectionRevenueInfo.RevenueCode, _appsettings.DepositOnConsentRevenue))
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "This type of revenue cannot be used"
                     });
                }
                else if (collectionRevenueInfo != null && (string.IsNullOrEmpty(collectionRevenueInfo.PayerUtin)) && (string.IsNullOrEmpty(collectionRevenueInfo.PayerId)))
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "This payment is yet to be normalized, kindly Normalize and try again"
                     }
                 );
                }
                else if (collectionRevenueInfo.PaymentBalance <= 0)
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "This payment has been used"
                     }
                 );
                }
                else if (collectionRevenueInfo.IsReversed == true)
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "This payment is not valid"
                     });
                }
                else if (!string.Equals(collectionRevenueInfo.PayerUtin, item.PayerUtin))
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "The payer Utin you supplied doesn't match the one in collection"
                     });
                }
                else if (!string.Equals(collectionRevenueInfo.RevenueCode, _appsettings.DepositOnConsentRevenue))
                {
                    response.Add(
                     new GetDepositResponseDto
                     {
                         PaymentReferenceNumber = item.PaymentRefNumber,
                         Error = "The revenue code you supplied doesn't match the one in collection"
                     });
                }
                else
                {
                    if (!string.IsNullOrEmpty(collectionRevenueInfo.AssessmentNo) && collectionRevenueInfo.PaymentBalance < 0)
                    {
                        response.Add(
                            new GetDepositResponseDto
                            {
                                PaymentReferenceNumber = item.PaymentRefNumber,
                                Error = "Payment owner does not match assessment owner."
                            });
                    }
                    response.Add(
                           new GetDepositResponseDto
                           {
                               PaymentReferenceNumber = item.PaymentRefNumber,
                               Amount = -collectionRevenueInfo.PaymentBalance
                           });
                }
            }
            if (response.Any(x => !string.IsNullOrEmpty(x.Error)))
            {
                return ApplicationConstants.FailureMessage<List<GetDepositResponseDto>>(response, "Some records could not be verified");
            }
            return ApplicationConstants.SuccessMessage<List<GetDepositResponseDto>>(response, "Success");
        }
        public string EmptyPdf()
        {
            var sbNoRecord = new StringBuilder();
            sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

            sbNoRecord.Append(@"</body>
                        </html>");

            return sbNoRecord.ToString();
        }
        public async Task<List<AssessmentDTO>> GetAllApprovedAssessmentNotice(string sentAgencyCode, string paymentCode)
        {
            var sources = new List<AssessmentDTO>();
            var result = new List<BillInfo>();
            try
            {
                var checkForMergeAssessment = await _context.BillInfo.Where(x => x.PaymentCode == paymentCode && x.IsAdditionalAssessmentRequired == true && x.IsPrimaryAssessment == true && x.MergerRequestId != null && x.IsDeleted != true).ToListAsync().ConfigureAwait(false);

                if (checkForMergeAssessment.Any())
                {
                    var querryWithoutAgencyCodeResult = await (from a in _context.BillInfo
                                                               join e in _context.BillDetails on a.BillId equals e.BillInfoGuid
                                                               join b in _context.Service on a.ServiceId equals b.Id
                                                               join c in _context.AgencySignature.DefaultIfEmpty() on a.SignatureId equals c.Id into agencySignatures
                                                               from c in agencySignatures
                                                               join d in _context.Agency.DefaultIfEmpty() on a.AgencyCode equals d.Code into agencies
                                                               from d in agencies
                                                               where (a.IsDeleted == false || a.IsDeleted == null)
                                                                  && a.IsApproved == true
                                                                  && (a.IsReversed == false || a.IsReversed == null)
                                                                  && (a.PaymentCode == paymentCode || a.BaseNumber == paymentCode)
                                                               select new AssessmentDTO
                                                               {
                                                                   Name = a.PayerName,
                                                                   PayerName = a.PayerName,
                                                                   ParentID = a.Id,
                                                                   RevenueName = b.ServiceRevenue.First().RevenueName,
                                                                   PaymentItem = b.ServiceRevenue.First().PaymentItemName,
                                                                   PaymentCode = a.PaymentCode,
                                                                   AssessementRefNo = a.PaymentCode,
                                                                   SignatoryName = b.SignatoryName,
                                                                   AgencyEmail = d.Email,
                                                                   AgencyPhone = d.Phone,
                                                                   AgencyName = d.Name,
                                                                   AgencyAddress1 = d.OfficialAddress1,
                                                                   AgencyAddress2 = d.OfficialAddress2,
                                                                   AgencyLogo = d.AgencyLogo.Image,
                                                                   AgencySignature = c.Image,
                                                                   AgencyCode = d.Code,
                                                                   PayerRefNo = a.PayerUtin,
                                                                   AssessementPeriod = a.BillPeriod,
                                                                   AssessmentBalance = a.TotalAssessed,
                                                                   BillInfoGuid = a.BillId,
                                                                   DateApproved = Convert.ToDateTime(a.ApprovedOn.Value.ToString("dd MMM yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo)),
                                                                   RequestId = a.BillId.ToString(),
                                                                   ServiceName = b.Name,
                                                                   RebatePercentage = Convert.ToInt32(a.BillDetails.First().RebatePercentage),
                                                                   IsMailSent = a.IsMailSent,
                                                                   SignatoryPosition = b.SignatoryPosition,
                                                                   ServiceType = e.TypeName,
                                                                   PartPaymentAllow = a.BillDetails.First().PartPaymentAllow,
                                                                   Address = a.Address,
                                                                   Telephone = a.Telephone,
                                                                   Email = a.Email,
                                                                   Location = a.BillDetails.First().LocationName,
                                                                   DateSubmitted = (DateTime)a.CreatedOn,
                                                                   ServiceId = a.BillDetails.First().ServiceId,
                                                                   DateCreated = a.CreatedOn,
                                                                   AssesementAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                                                   RebateAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().RebateAmount).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                                                   AssessmentBalanceFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().BillBalance).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                                                   TotalAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed) + Convert.ToDecimal(a.BillDetails.First().RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                                                   AssesementAmountpaid = a.BillDetails.First().BillAmountPaid,
                                                               }).ToListAsync().ConfigureAwait(false);

                    // Now, fetch and assign BillDetails separately
                    foreach (var dto in querryWithoutAgencyCodeResult)
                    {
                        dto.BillDetails = await _context.BillDetails
                            .Where(x => x.BillInfoGuid == dto.BillInfoGuid)
                            .Select(x => new BillDetailsDto // map to DTO here
                            {
                                Id = x.Id,
                                PaymentItemName = x.PaymentItemName,
                                RebateAmount = x.RebateAmount,
                                // add other fields as needed
                            }).ToListAsync();
                    }


                        sources = querryWithoutAgencyCodeResult;

                    return sources;
                }

                var querryResult = await (from a in _context.BillInfo
                                          join e in _context.BillDetails on a.BillId equals e.BillInfoGuid
                                          join b in _context.Service on a.ServiceId equals b.Id
                                          join c in _context.AgencySignature.DefaultIfEmpty() on a.SignatureId equals c.Id into agencySignatures
                                          from c in agencySignatures
                                          join d in _context.Agency.DefaultIfEmpty() on a.AgencyCode equals d.Code into agencies
                                          from d in agencies
                                          where (a.IsDeleted == false || a.IsDeleted == null)
                                             && a.IsApproved == true
                                             && (a.IsReversed == false || a.IsReversed == null)
                                             && (a.PaymentCode == paymentCode || a.BaseNumber == paymentCode)
                                          select new AssessmentDTO
                                          {
                                              Name = a.PayerName,
                                              PayerName = a.PayerName,
                                              ParentID = a.Id,
                                              RevenueName = b.ServiceRevenue.First().RevenueName,
                                              PaymentItem = b.ServiceRevenue.First().PaymentItemName,
                                              PaymentCode = a.PaymentCode,
                                              AssessementRefNo = a.PaymentCode,
                                              SignatoryName = b.SignatoryName,
                                              AgencyEmail = d.Email,
                                              AgencyPhone = d.Phone,
                                              AgencyName = d.Name,
                                              AgencyAddress1 = d.OfficialAddress1,
                                              AgencyAddress2 = d.OfficialAddress2,
                                              AgencyLogo = d.AgencyLogo.Image,
                                              AgencySignature = c.Image,
                                              AgencyCode = d.Code,
                                              PayerRefNo = a.PayerUtin,
                                              AssessementPeriod = a.BillPeriod,
                                              AssessmentBalance = a.TotalAssessed,
                                              BillInfoGuid = a.BillId,
                                              DateApproved = Convert.ToDateTime(a.ApprovedOn.Value.ToString("dd MMM yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo)),
                                              RequestId = a.BillId.ToString(),
                                              ServiceName = b.Name,
                                              RebatePercentage = Convert.ToInt32(a.BillDetails.First().RebatePercentage),
                                              IsMailSent = a.IsMailSent,
                                              SignatoryPosition = b.SignatoryPosition,
                                              ServiceType = e.TypeName,
                                              PartPaymentAllow = a.BillDetails.First().PartPaymentAllow,
                                              Address = a.Address,
                                              Telephone = a.Telephone,
                                              Email = a.Email,
                                              Location = a.BillDetails.First().LocationName,
                                              DateSubmitted = (DateTime)a.CreatedOn,
                                              ServiceId = a.BillDetails.First().ServiceId,
                                              DateCreated = a.CreatedOn,
                                              AssesementAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                              RebateAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().RebateAmount).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                              AssessmentBalanceFormatted = Convert.ToDecimal(Convert.ToDecimal(a.BillDetails.First().BillBalance).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                              TotalAmountFormatted = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(a.TotalAssessed) + Convert.ToDecimal(a.BillDetails.First().RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N"),
                                          }).ToListAsync().ConfigureAwait(false);

                    foreach (var dto in querryResult)
                    {
                        dto.BillDetails = await _context.BillDetails
                            .Where(x => x.BillInfoGuid == dto.BillInfoGuid)
                            .Select(x => new BillDetailsDto // map to DTO here
                            {
                                Id = x.Id,
                                PaymentItemName = x.PaymentItemName,
                                RebateAmount = x.RebateAmount,
                                // add other fields as needed
                            }).ToListAsync();
                    }
                        sources = querryResult;

                return sources;
            }
            catch (Exception ex)
            {
                //var errorMsg = ExceptionExtensions.Output(ex);
                Log.Error($"An Exception Error has occurred in ({nameof(GetAllApprovedAssessmentNotice)}) with the following details : - {ex.Message}");
            }
            return sources;
        }
        public async Task<List<AssessmentsDto>> GetAllApprovedAssessmentNotices(string sentAgencyCode, string paymentCode)
        {
            var sources = new List<AssessmentsDto>();
            var result = new List<BillInfo>();
            try
            {
                var checkForMergeAssessment = await _context.BillInfo.Where(x => x.PaymentCode == paymentCode && x.IsAdditionalAssessmentRequired == true && x.IsPrimaryAssessment == true && x.MergerRequestId != null && x.IsDeleted != true).ToListAsync().ConfigureAwait(false);

                if (checkForMergeAssessment.Any())
                {
                    var querryWithoutAgencyCodeResult = await (from a in _context.BillInfo
                                                               join e in _context.BillDetails on a.BillId equals e.BillInfoGuid
                                                               join b in _context.Service on a.ServiceId equals b.Id
                                                               join c in _context.AgencySignature.DefaultIfEmpty() on a.SignatureId equals c.Id into agencySignatures
                                                               from c in agencySignatures
                                                               join d in _context.Agency.DefaultIfEmpty() on c.AgencyCode equals d.Code into agencies
                                                               from d in agencies
                                                               where (a.IsDeleted == false || a.IsDeleted == null)
                                                                  && a.IsApproved == true
                                                                  && (a.IsReversed == false || a.IsReversed == null)
                                                                  && a.PaymentCode == paymentCode
                                                               select new AssessmentsDto
                                                               {
                                                                   Name = a.PayerName,
                                                                   ParentID = a.Id,
                                                                   RevenueName = b.ServiceRevenue.First().RevenueName,
                                                                   PaymentItem = b.ServiceRevenue.First().PaymentItemName,
                                                                   PaymentCode = a.PaymentCode,
                                                                   AssessementRefNo = a.PaymentCode,
                                                                   SignatoryName = b.SignatoryName,
                                                                   AgencyEmail = d.Email,
                                                                   AgencyPhone = d.Phone,
                                                                   AgencyName = d.Name,
                                                                   AgencyAddress1 = d.OfficialAddress1,
                                                                   AgencyAddress2 = d.OfficialAddress2,
                                                                   AgencyLogo = d.AgencyLogo.Image,
                                                                   AgencySignature = c.Image,
                                                                   PayerRefNo = a.PayerUtin,
                                                                   AssessementPeriod = a.BillPeriod,
                                                                   AssessmentBalance = a.TotalAssessed,
                                                                   BillInfoGuid = a.BillId,
                                                                   DateApproved = Convert.ToDateTime(a.ApprovedOn.Value.ToString("dd MMM yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo)),
                                                                   RequestId = a.BillId.ToString(),
                                                                   ServiceName = b.Name,
                                                                   RebatePercentage = Convert.ToInt32(a.BillDetails.First().RebatePercentage),
                                                                   IsMailSent = a.IsMailSent,
                                                                   PartPaymentAllow = a.BillDetails.First().PartPaymentAllow,
                                                                   Address = a.Address,
                                                                   Telephone = a.Telephone,
                                                                   Email = a.Email,
                                                                   Location = _appsettings.StateName,   //a.BillDetails.First().LocationName,
                                                                   DateSubmitted = a.CreatedOn,
                                                                   ServiceId = a.BillDetails.First().ServiceId
                                                               }).ToListAsync().ConfigureAwait(false);



                    sources = querryWithoutAgencyCodeResult;

                    return sources;
                }

                var querryResult = await (from a in _context.BillInfo
                                          join e in _context.BillDetails on a.BillId equals e.BillInfoGuid
                                          join b in _context.Service on a.ServiceId equals b.Id
                                          join c in _context.AgencySignature
                                              on a.AgencyCode equals c.AgencyCode into agencySignatures
                                          from c in agencySignatures.DefaultIfEmpty() // Left outer join
                                          join d in _context.Agency on c.AgencyCode equals d.Code into agencies
                                          from d in agencies.DefaultIfEmpty() // Left outer join
                                          where (a.IsDeleted == null || a.IsDeleted == false)
                                             && a.IsApproved == true
                                             && (a.IsReversed == false || a.IsReversed == null)
                                             && a.AgencyCode == sentAgencyCode
                                             && a.BaseNumber == paymentCode
                                          select new AssessmentsDto
                                          {
                                              Name = a.PayerName,
                                              ParentID = a.Id,
                                              RevenueName = b.ServiceRevenue.First().RevenueName,
                                              PaymentItem = b.ServiceRevenue.First().PaymentItemName,
                                              PaymentCode = a.PaymentCode,
                                              AssessementRefNo = a.PaymentCode,
                                              SignatoryName = b.SignatoryName,
                                              AgencyEmail = d.Email,
                                              AgencyPhone = d.Phone,
                                              AgencyName = d.Name,
                                              AgencyAddress1 = d.OfficialAddress1,
                                              AgencyAddress2 = d.OfficialAddress2,
                                              AgencyLogo = d.AgencyLogo.Image,
                                              AgencySignature = c.Image,
                                              PayerRefNo = a.PayerUtin,
                                              AssessementPeriod = a.BillPeriod,
                                              AssessmentBalance = a.TotalAssessed,
                                              BillInfoGuid = a.BillId,
                                              DateApproved = Convert.ToDateTime(a.ApprovedOn.Value.ToString("dd MMM yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo)),
                                              RequestId = a.BillId.ToString(),
                                              ServiceName = b.Name,
                                              RebatePercentage = Convert.ToInt32(a.BillDetails.First().RebatePercentage),
                                              IsMailSent = a.IsMailSent,
                                              PartPaymentAllow = a.BillDetails.First().PartPaymentAllow,
                                              Address = a.Address,
                                              Telephone = a.Telephone,
                                              Email = a.Email,
                                              Location = _appsettings.StateName,  //a.BillDetails.First().LocationName,
                                              DateSubmitted = a.CreatedOn,
                                              ServiceId = a.ServiceId
                                          }).ToListAsync().ConfigureAwait(false);

                sources = querryResult;

                return sources;
            }
            catch (Exception ex)
            {
                //var errorMsg = ExceptionExtensions.Output(ex);
                Log.Error($"An Exception Error has occurred in ({nameof(GetAllApprovedAssessmentNotice)}) with the following details : - {ex.Message}");
            }
            return sources;
        }
        public async Task<string> GenerateConfirmDocBarcode(string urlpath)
        {
            try
            {
                var barCodeRequest = new BarCodeRequest
                {
                    rawString = urlpath
                };

                if (string.IsNullOrEmpty(urlpath))
                    return "";

                var response = await _genericHttpClientHandlerService.PostAsync<BarCodeRequest, BarCodeResponse>($"{_appsettings.BarcodeServiceUrl}{ApplicationConstants.GenerateBarcodeMethod}", barCodeRequest);

                if (response != null && response.succeeded)
                {
                    return response.data.qrImageUrl;
                }
                return "";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return ex.InnerException == null ? "Error occured while processing your request" : ex.InnerException.Message;
            }
        }
        public async Task<List<BillAdditionalInfoDto>> GetAllAdditionalInfo(Guid billInfoGuid)
        {
            try
            {
                var additionalInfo = await (from a in _context.BillAdditionalInfo
                                            where a.BillInfoId == billInfoGuid && (a.IsDeleted == null || a.IsDeleted == false)
                                            join c in _context.AdditionalServiceDetail on a.AdditionalServiceDetailId equals c.Id
                                            select new BillAdditionalInfoDto
                                            {
                                                RequestId = a.AdditionalInfoId.ToString(),
                                                FieldName = c.AdditionalServiceDetailName,
                                                FieldValue = a.FieldValue,
                                                AdditionalServiceDetailId = a.AdditionalServiceDetailId,
                                            }).ToListAsync().ConfigureAwait(false);
                return additionalInfo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<BillAdditionalInfoDto>> GetAdditionalInfoByBillInfoId(Guid billInfoGuid)
        {
            try
            {
                var additionalInfo = await (from a in _context.BillAdditionalInfo.Where(x => x.BillInfoId == billInfoGuid)
                                            join b in _context.AdditionalServiceDetail on a.AdditionalServiceDetailId equals b.Id
                                            select new BillAdditionalInfoDto
                                            {
                                                AdditionalServiceDetailName = b.AdditionalServiceDetailName,
                                                AdditionalServiceDetailId = b.Id,
                                                FieldValue = a.FieldValue,
                                                AdditionalInfoId = a.AdditionalInfoId,
                                                BillInfoId = a.BillInfoId,
                                                CreatedOn = a.CreatedOn,
                                                CreatedBy = a.CreatedBy,
                                                LastUpdated = a.LastUpdated,
                                                UpdatedBy = a.UpdatedBy,
                                                IsDeleted = a.IsDeleted,
                                                DeletedOn = a.DeletedOn,
                                                DeletedBy = a.DeletedBy
                                            }).ToListAsync().ConfigureAwait(false);

                if (additionalInfo == null || additionalInfo.Count < 1)
                {
                    var billInfo = await _context.BillInfo.FirstOrDefaultAsync(x => x.BillId == billInfoGuid);
                    additionalInfo = await _context.ServiceDetails
                                    .Where(x => x.ServiceId == billInfo.ServiceId)
                                    .Select(a => new BillAdditionalInfoDto
                                    {
                                        FieldName = a.DetailName,
                                        AdditionalServiceDetailId = (int)a.AdditionalInfoId,
                                        AdditionalServiceDetailName = a.DetailName,
                                        CreatedOn = a.CreatedOn,
                                        BillInfoId = billInfoGuid,
                                        CreatedBy = a.CreatedBy,
                                        LastUpdated = a.LastUpdated,
                                        UpdatedBy = a.UpdatedBy,
                                        IsDeleted = a.IsDeleted,
                                        DeletedOn = a.DeletedOn,
                                        DeletedBy = a.DeletedBy
                                    }).ToListAsync();
                }

                return additionalInfo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GetAssessmentsNoticeHtmlString(List<AssessmentsDto> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var payerFileNo = string.Empty;
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   Opps! No Record to Display, due to Invalid Assessment Record. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            //var parentAssessmentDetail = assessmentDetail.Where(x => x.PartPaymentAllow == false).FirstOrDefault();
            var assessmentDetails = assessmentDetail.Where(x => x.PartPaymentAllow == true);
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower()); //TitleCase
            var payerstate = string.IsNullOrEmpty(assessmentDetailFirstDefault.Location) ? "NOT PROVIDED" : assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "ASSESSMENT NOTICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var agencyName = string.IsNullOrEmpty(assessmentDetailFirstDefault.AgencyName) ? null : assessmentDetailFirstDefault.AgencyName?.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1?.ToUpperInvariant();
            var agencyAddress2 = string.IsNullOrEmpty(assessmentDetailFirstDefault.AgencyAddress2) ? null : assessmentDetailFirstDefault.AgencyAddress2?.ToUpperInvariant();
            var agencyLogo = _context.AgencyLogo.FirstOrDefaultAsync().Result.Image;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = string.IsNullOrEmpty(assessmentDetailFirstDefault.ServiceName) ? null : assessmentDetailFirstDefault.ServiceName.ToUpperInvariant();
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var offerShortName = "assessment";

            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }

            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div class='thirdheader' style='text-align: justify;'>You are required to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable on this {offerShortName}. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS at Point of Service.</div>";
            //&nbsp;&nbsp;&nbsp;&nbsp;
            var LandText = $"For enquires on this bill, kindly call {assessmentDetailFirstDefault.AgencyPhone} or send an email to {assessmentDetailFirstDefault.AgencyEmail}.<br /><br /> Many thanks.";

            var standardLetterDescriptions = LandText; // assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} IN LAND SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()} {payerstate}.";

            //var firstBodyWord = $"Refer to the above subject matter, <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b> &nbsp;has graciously considered your request and approved the {serviceSubHeader}";
            var firstBodyWord = "Refer to the above subject matter and find below the detailed assessment items for payment.";
            //firstBodyWord = ShowLineBreaks(firstBodyWord);
            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            //sb.AppendFormat(@$"<div style='text-align: center;'><img class='imageRight' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/> <br><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div>");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div>");

            sb.AppendFormat(@"<html>
                            <head>
                            </head>
                            <body style='width: 97%'>
                        <div class='flex-container'>
                                    <div>
                                        <label class='label2'>{0}</label>
                                    </div>
                           </div><br>", payerFileNo);

            if (!string.IsNullOrEmpty(salutationName))
            {
                sb.Append(@$"{salutationName} <br>");
            }

            if (!string.IsNullOrEmpty(assigneeName))
            {
                sb.Append(@$"{assigneeName} <br>");
                sb.Append(@"C/o");
            }

            sb.AppendFormat(@"
                        <div class='row'>
                            <div class='column'> {0}<br> {1}</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{2} <br>Phone No.: {3}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{4}</q> <br> <span class='secondheader'>{5}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{6}</div> <br>
                            
                        </div>"
                            , salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords);


            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();
                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'><b>{1}</b></td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td><b>Total</b></td>
                            <td align='right'><b>{0}</b>&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }


            sb.Append(@"</table>");

            //sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/>
                                
                            ", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<div style='text-align: left; padding-top: 10px;'>{0}</div>
                                <p style='text-align: left;'> {1} <br> {2} <br> <b>{3}</b></p>
                            </html>", agencySignature, signatoryName, signatoryPosition, agencyName);

            return sb.ToString();
        }
        public async Task<string> GenerateBarcode(string urlpath)
        {
            var sources = string.Empty;
            const string serviceMethod = "/barcode/api/v1/QrCoder/encode-string";
            var url = _appsettings.BarcodeServiceUrl + serviceMethod;
            var cachedData = _cache.Get<string>("paymentInstructionsGenerateBarcodeCacheKey");
            if (!string.IsNullOrEmpty(cachedData))
            {
                // Return cached data
                sources = cachedData;
                return sources;
            }

            if (!string.IsNullOrEmpty(urlpath))
            {
                var barCodeRequest = new BarCodeRequest
                {
                    rawString = urlpath
                };
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
                var contents = new StringContent(JsonConvert.SerializeObject(barCodeRequest), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, contents);
                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    BarCodeResponse myDeserializedClass = JsonConvert.DeserializeObject<BarCodeResponse>(apiResponse);
                    sources = myDeserializedClass.data.qrImageUrl;

                    _cache.Set("paymentInstructionsGenerateBarcodeCacheKey", sources, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = AppWebExtension.CalculateCacheExpirationTime()
                    });

                    return sources;
                }

                return sources;
            }

            return sources;
        }
        public async Task<List<BillDetailsDto>> GetBillDetail(Guid billInfoGuid)
        {
            var list = await _context.BillDetails
                .Where(x => x.BillInfoGuid == billInfoGuid)
                .ToListAsync();

            var mappedList = _mapper.Map<List<BillDetailsDto>>(list);

            return mappedList;
        }
        public string AssessmentEmptyPdf()
        {
            var sbNoRecord = new StringBuilder();
            sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   Opps! No Record to Display, due to Invalid Assessment Record. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

            sbNoRecord.Append(@"</body>
                        </html>");

            return sbNoRecord.ToString();
        }
        public string GetAssessmentNoticeHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, string howToPayUrl, string confirmDocUrl)
        {
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var payerFileNo = string.Empty;
            var signature = $"{_appsettings.SignatureUrl}";
            var stateLogoUrl = $"{_appsettings.StateLogoUrl}";

            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   Opps! No Record to Display, due to Invalid Assessment Record. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>"
                );
                sbNoRecord.Append(@"</body></html>");
                return sbNoRecord.ToString();
            }

            var htmlTemplate = Path.Combine(Directory.GetCurrentDirectory(), "PdfGenerator", "HtmlTemplates", "AssessmentNotice.html");

            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var assessmentDetails = assessmentDetail.Where(x => x.PartPaymentAllow == true);
            var html = File.ReadAllText(htmlTemplate);
            var headerFormat = _context.LetterFormat.FirstOrDefaultAsync(x => x.AgencyCode == assessmentDetailFirstDefault.AgencyCode).Result;

            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {
                    
            }
        
            string rows = "";
            decimal? totalAmount = 0;

            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)assessmentDetailFirstDefault.BillDetails.Sum(x => x.TotalBillAmount));
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var sumOfRebate = assessmentDetailFirstDefault.BillDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(assessmentDetailFirstDefault.BillDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalAfterRebate = (assessmentDetailFirstDefault.BillDetails.Sum(x => x.TotalBillAmount)) - (assessmentDetailFirstDefault.BillDetails.Sum(x => x.RebateAmount));
            var totalAfterRebateInWord = NumberToWordsConverter.ConvertToWords((decimal)totalAfterRebate);
            var formattedTotalAfterRebate = Convert.ToDecimal(Convert.ToDecimal(totalAfterRebate).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(assessmentDetailFirstDefault.BillDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");



            var tableHead = "";
            var tableFoot = "";
            if (sumOfRebate > 0)
            {
                var rebatePercentage = assessmentDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();
                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = assessmentDetails.Where(x => x.RebateAmount > 0 && x.ShowRebatePercentage == false).Any();



                tableHead = $@"<th align='left' style=""font-size: 23px !important;"">&nbsp;Payment Item(s)</th>
                                   <th align='right' style=""font-size: 23px !important;"">Amount (₦)</th>
                                   <th align='right' style=""font-size: 23px !important;"">" + (showRebatePercentStatus ? $"{rebatePercentage} % Rebate (₦)" : "Rebate (₦)") + @"</th>
                                   <th align='right' style=""font-size: 23px !important;"">Total (₦)</th>";

                foreach (var detail in assessmentDetailFirstDefault.BillDetails)
                {
                    var billAmount = Convert.ToDecimal(Convert.ToDecimal(detail.BillAmount));
                    var rebateAmount = Convert.ToDecimal(Convert.ToDecimal(detail.RebateAmount));
                    var totalAmountLessRebateAmount = billAmount - rebateAmount;

                    rows += $@"<tr align='left' style=""font-size: 23px !important;"">
                               <td align='left' style=""font-size: 23px !important;"">{detail.PaymentItemName}</td>
                               <td align='right' style=""font-size: 23px !important;"">&nbsp;{billAmount}&nbsp;</td>
                               <td align='right' style=""font-size: 23px !important;"">&nbsp;{rebateAmount}&nbsp;</td>
                               <td align='right' style=""font-size: 23px !important;"">&nbsp;{Convert.ToDecimal(Convert.ToDecimal(totalAmountLessRebateAmount))}&nbsp;</td></tr>";
                }

                tableFoot = $@" <td style='font-weight: bolder;font-size: 23px !important;'>Total</td>
                            <td align='right' style='font-weight: bolder;font-size: 23px !important;'>{sumOfAssAmountFormated}</td>
                            <td align='right' style='font-weight: bolder;font-size: 23px !important;'><b>{sumOfRebateFormated}</b></td>
                            <td align='right' style='font-weight: bolder;font-size: 23px !important;'>{formattedTotalAfterRebate}&nbsp;</td>";


                html = html.Replace("{{totalAmount}}", Convert.ToDecimal(Convert.ToDecimal(totalAfterRebate).ToString(CultureInfo.InvariantCulture)).ToString("N"));
                html = html.Replace("{{amountInWords}}", totalAfterRebateInWord);
            }
            else
            {
                tableHead = @"<th align='left' style=""font-size: 23px !important;"">&nbsp;Payment Item(s)</th>
                              <th align='right' style=""font-size: 23px !important;"">Amount (₦)&nbsp;</th>";

                foreach (var detail in assessmentDetailFirstDefault.BillDetails)
                {
                    var billAmount = Convert.ToDecimal(detail.BillAmount).ToString(CultureInfo.InvariantCulture);
                    rows += $@"
                    <tr>
                        <td style='text-align: left; font-size: 23px !important;'>{detail.PaymentItemName}</td>
                        <td style='text-align: right; font-size: 23px !important;'>{Convert.ToDecimal(billAmount).ToString("N")}</td>
                    </tr>";
                    totalAmount += detail.BillAmount;
                }

                tableFoot = $@"<th style='font-size: 23px;'>Total</th>
                <th style='text-align: right; font-size: 23px;'>{Convert.ToDecimal(Convert.ToDecimal(assessmentDetailFirstDefault.BillDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N")}</th>";

                html = html.Replace("{{totalAmount}}", Convert.ToDecimal(Convert.ToDecimal(assessmentDetailFirstDefault.BillDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N"));
                html = html.Replace("{{amountInWords}}", amountInWords);
            }

            html = html.Replace("{{agencyName}}", $"{assessmentDetailFirstDefault.AgencyName.ToUpperInvariant()}");
            html = html.Replace("{{tableHead}}", tableHead);
            html = html.Replace("{{noticeTitle}}", _appsettings.NoticeTitle);
            html = html.Replace("{{confirmDocumentBarcode}}", confirmDocBarcode);
            html = html.Replace("{{howToPayBarcode}}", howToPayBarcode);
            html = html.Replace("{{stateLogo}}", stateLogoUrl);
            html = html.Replace("{{agencyAddress}}", $"{agencyAddress1} {agencyAddress2}");
            html = html.Replace("{{paymentCode}}", assessmentDetailFirstDefault.PaymentCode);
            html = html.Replace("{{rows}}", rows);
            html = html.Replace("{{salutation}}", salutationName);
            html = html.Replace("{{payerName}}", assessmentDetailFirstDefault.PayerName);
            html = html.Replace("{{fileNo}}", payerFileNo);
            html = html.Replace("{{stateName}}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_appsettings.StateName.ToLowerInvariant()));
            html = html.Replace("{{paymentPlatforms}}", _appsettings.PaymentPlatformNames);
            html = html.Replace("{{onlinePaymentUrl}}", _appsettings.PaymentOnlineWebsite);
            html = html.Replace("{{inquiryPhoneNo}}", assessmentDetailFirstDefault.AgencyPhone);
            html = html.Replace("{{agencyEmail}}", assessmentDetailFirstDefault.AgencyEmail);
            html = html.Replace("{{serviceSignatoryName}}", assessmentDetailFirstDefault.SignatoryName);
            html = html.Replace("{{serviceName}}", assessmentDetailFirstDefault.ServiceName.ToUpperInvariant());
            html = html.Replace("{{payerAddress}}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower()));
            html = html.Replace("{{signatoryPosition}}", signatoryPosition);
            html = html.Replace("{{serviceSignatoryName}}", signatoryName);
            html = html.Replace("{{payerPhoneNo}}", assessmentDetailFirstDefault.Telephone);
            html = html.Replace("{{serviceType}}", assessmentDetailFirstDefault.ServiceType);
            html = html.Replace("{{assessmentDate}}", assessmentDetailFirstDefault.DateSubmitted.ToLongDateString());
            html = html.Replace("{{tableFoot}}", tableFoot);
            if (headerFormat != null && !string.IsNullOrEmpty(headerFormat.HeaderName))
            {
                html = html.Replace("{{headerName}}", headerFormat.HeaderName);
                html = html.Replace("<span class=\"headerNameCls\" style=\"display:none;\">", "<span class=\"propLoc\">");
            }

            if (headerFormat != null && headerFormat.IsAddressIncluded)
            {
                html = html.Replace("{{propertyLocation}}", propertyLocation.ToUpper());
                html = html.Replace("<span class=\"propLoc\" style=\"display:none;\">", "<span class=\"propLoc\">");
            }

            return html;
        }
        public string GetConsumptionTaxAssessmentNoticeHtml(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, string howToPayUrl, string confirmDocUrl)
        {
            var htmlTemplate = Path.Combine(Directory.GetCurrentDirectory(), "PdfGenerator", "HtmlTemplates", "ConsumptionTaxNotice.html");
            var signature = $"{_appsettings.SignatureUrl}";
            var stateLogoUrl = $"{_appsettings.StateLogoUrl}";
            var resultFirst = assessmentDetail.OrderBy(x => x.Id).First();
            var assessmentDate = resultFirst.DateCreated.Value.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            var salutationName = string.Empty;
            var html = File.ReadAllText(htmlTemplate);
            decimal? totalAssessed = assessmentDetail.SelectMany(x => x.BillDetails).Sum(x => x.Liability);
            string? configSignature = "";
            string signatureToUse = string.Format(configSignature.ObjectToString()).Replace("~", "");

            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation); if (salutationInDb.Any())
            {
                var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
                if (!string.IsNullOrEmpty(salutationValue))
                {

                }
            }
            var totalLiability = string.Format(totalAssessed.ToString()).Replace(",", "");
            var totalLiabilityInWords = NumberToWordsConverter.ConvertToWords(Convert.ToDecimal(totalLiability));
            var payerAddress = string.IsNullOrEmpty(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resultFirst.Address.ToLower())) ? "Not Provided at the Point of Registration." : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resultFirst.Address.ToLower());

            string rows = "";
            decimal? totalAmount = 0;
            foreach (var item in assessmentDetail)
            {
                foreach (var details in item.BillDetails)
                {
                    rows += $@"
                    <tr>
                       
                        <td style='text-align: left;'>{details.PaymentItemName}</td>
                        <td style='text-align: right;'>{(details.BillAmount == null ? 0 : details.BillAmount):N2}</td>
                        <td style = 'text-align: right;'>{(details.Liability == null ? 0 : details.Liability):N2}</td>
                        <td style = 'text-align: right;'>{((details.BillAmount == null ? 0 : details.BillAmount) + (details.Liability == null ? 0 : details.Liability)):N2}</td>
                    </tr>";
                    totalAmount += (details.BillAmount == null ? 0 : details.BillAmount) + (details.Liability == null ? 0 : details.Liability);
                }
            }

            html = html.Replace("{{amountInWords}}", $"{totalLiabilityInWords} Only (₦ {string.Format("{0:n2}", totalAmount)})");
            html = html.Replace("{{totalPayment}}", $"{string.Format("{0:n2}", totalAmount)}");

            html = html.Replace("{{rows}}", rows);
            html = html.Replace("{{stateLogo}}", stateLogoUrl);
            html = html.Replace("{{backgroundLogo}}", stateLogoUrl);
            html = html.Replace("{{consolidatedAssessmentRefNumber}}", resultFirst.BaseAssnumber);
            html = html.Replace("{{assessmentDate}}", string.Format("{0:D}", resultFirst.DateCreated));
            html = html.Replace("{{salutation}}", salutationName);
            html = html.Replace("{{taxAgentName}}", resultFirst.PayerName);
            html = html.Replace("{{taxAgentAddress}}", payerAddress);
            html = html.Replace("{{itemRefNumberSample}}", resultFirst.BaseAssnumber);
            html = html.Replace("{{taxYear}}", resultFirst.TransactionYear.ObjectToString());
            html = html.Replace("{{agencyAddress}}", $"{resultFirst?.AgencyAddress.ObjectToString()} {resultFirst?.AgencyAddress1.ObjectToString()}");
            html = html.Replace("{{agencyOffice1}}", resultFirst?.AgencyAddress1.ObjectToString());
            html = html.Replace("{{agencyOffice2}}", resultFirst?.AgencyAddress2.ObjectToString());
            html = html.Replace("{{howToPayBarcode}}", howToPayUrl);
            html = html.Replace("{{confirmBarcode}}", confirmDocUrl);
            html = html.Replace("{{agencyEmail}}", resultFirst.AgencyEmail);
            html = html.Replace("{{agencyPhoneNumber}}", resultFirst?.AgencyPhone.ObjectToString());
            html = html.Replace("{{paymentPlatformNames}}", _appsettings.PaymentPlatformNames);
            html = html.Replace("{{demandNoticeGracePeriod}}", _appsettings.DemandNoticeGracePeriod);
            html = html.Replace("{{stateName}}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_appsettings.StateName.ToLowerInvariant()));
            html = html.Replace("{{signatoryOffice}}", resultFirst.Location == null ? "Not Provided" : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resultFirst?.Location?.ToLowerInvariant()));
            html = html.Replace("{{paymentOnlineWebsite}}", _appsettings.PaymentOnlineWebsite);
            html = html.Replace("{{agencyNameHeading}}", resultFirst.AgencyName);
            html = html.Replace("{{paymentCode}}", resultFirst.PaymentCode);
            html = html.Replace("{{PayerID}}", resultFirst.PayerRefNo);
            html = html.Replace("{{payerName}}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resultFirst.PayerName.ToUpperInvariant()));
            html = html.Replace("{{payerAddress}}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resultFirst.Address.ToLowerInvariant()));
            html = html.Replace("{{assessmentRefNo}}", resultFirst.AssessementRefNo);
            html = html.Replace("{{payerPhone}}", resultFirst.AgencyPhone);
            html = html.Replace("{{signatoryName}}", resultFirst.SignatoryName);
            html = html.Replace("{{signatoryPosition}}", resultFirst.SignatoryPosition);
            html = html.Replace("{{agencyname}}", resultFirst.AgencyName);
            html = html.Replace("{{payerId}}", resultFirst.PayerRefNo);
            html = html.Replace("{{payerId}}", resultFirst.PayerRefNo);
            html = html.Replace("{{selfServiceUrl}}", _appsettings.SelfServiceUrl);
            html = html.Replace("{{complaintEmail}}", _appsettings.ComplaintEmail);
            html = html.Replace("{{paymentOnlineUrl}}", _appsettings.PaymentOnlineWebsite);
            html = html.Replace("{{paymentPlatForms}}", _appsettings.PaymentPlatformNames);
            html = html.Replace("{{noticePeriodNumber}}", _appsettings.NoticePeriodNumber.ToString());
            html = html.Replace("{{complaintPeriod}}", _appsettings.ComplaintPeriod.ToString());
            html = html.Replace("{{stateName}}", _appsettings.StateName.ToString());
            html = html.Replace("{{noticePeriodWords}}", NumberToWordsConverter.ConvertToWords(_appsettings.NoticePeriodNumber).Replace(" Naira", ""));
            html = html.Replace("{{netPayableInWords}}", NumberToWordsConverter.ConvertToWords((decimal)totalAmount));
            html = html.Replace("{{signature}}", signature);
            html = html.Replace("{{paymentCode}}", resultFirst.PaymentCode);
            Log.Information("HTML Template Response [S]: {@Request}", html);
            return html;
        }
        public async Task<byte[]> ToPdf(Application.DTOs.PdfConverterRequest request)
        {

            _context.AdditionalServiceDetail.CountAsync(x => x.AdditionalServiceDetailName.StartsWith("fn"));


            var payload = new PdfConvertPayLoadRequest
            {
                ConverterTypeId = _appsettings.ConvertTypeId,
                DocumentName = request.OutputFileName,
                HtmlContentOrUrlPath = request.HtmlContent,
                PaperSize = "A4",
                Orientation = "Portrait"

            };
            byte[] fileBytesArray = null;
            try
            {
                var response = await _genericHttpClientHandlerService.PostPdfConvertAsync<PdfConvertPayLoadRequest>(_appsettings.PdfConverterUrl, payload);
                if (response != null)
                {
                    fileBytesArray = response;
                }
            }
            catch (Exception exception)
            {
                Log.Fatal($"Error when trying to convert URL request {request} to PDF in the {nameof(ToPdf)} Class - {exception.Message}");

                return Array.Empty<byte>();
            }
            return fileBytesArray;
        }

        public async Task<Response<string>> UpdateBillAdditionalInfo(List<UpdateBillAdditionalInfoRequestDto> request)
        {
            try
            {
                int success = 0;
                int fail = 0;
                foreach (var item in request)
                {
                    var additionalInfo = await _context.BillAdditionalInfo.FirstOrDefaultAsync(x => x.AdditionalInfoId == item.AdditionalInfoId);
                    if (additionalInfo != null)
                    {
                        additionalInfo.AdditionalServiceDetailId = item.AdditionalServiceDetailId;
                        additionalInfo.FieldValue = item.FieldValue;
                        additionalInfo.LastUpdated = DateTime.Now;
                        additionalInfo.UpdatedBy = _authenticatedUser.UserId;

                        _context.BillAdditionalInfo.Update(additionalInfo);
                    }
                    else
                    {
                        var addNewRecord = new BillAdditionalInfo
                        {
                            AdditionalServiceDetailId = item.AdditionalServiceDetailId,
                            BillInfoId = item.BillInfoId,
                            FieldValue = item.FieldValue,
                            CreatedOn = DateTime.Now,
                            CreatedBy = _authenticatedUser.UserId
                        };
                        await _context.BillAdditionalInfo.AddAsync(addNewRecord);
                    }

                    var save = await _context.SaveChangesAsync();

                    if (save > 0)
                    {
                        success++;
                    }
                    else
                    {
                        fail++;
                    }
                }

                return ApplicationConstants.SuccessMessage($"{success} record(s) updated, {fail} record(s) failed to update");
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message);
                throw ex;
            }
        }
        public string GetAllocationHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var payerFileNo = string.Empty;
            var sizeOfLand = string.Empty;
            var termsofGrant = string.Empty;
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower()); //TitleCase
            var payerstate = assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "WITHOUT PREJUDICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var secondSignatoryPosition = "For: Special Adviser/Director General (Lands)";
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = assessmentDetailFirstDefault.ServiceHeader;
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var agencyName = assessmentDetailFirstDefault.AgencyName.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var agencyLogo = assessmentDetailFirstDefault.AgencyLogo;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            var offerShortName = "allocation";

            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }
            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div class='thirdheader' style='text-align: justify;'><br /> 2. You are requested to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable on the {offerShortName} as evidence of acceptance of this offer. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS.</div>";
            //&nbsp;&nbsp;&nbsp;&nbsp;
            var LandText = $"3. The allocation is also subject to other implied and express conditions to be stated on the Certificate of Occupancy (that will be issued in evidence of this grant) and the Land Use Act CAP L5 LFN 2004. <br /><br /> 4. That you (applicant) should notify the Bureau upon full payments of your land charges for further processing of your title documents within the possible time frame. <br /><br /> 6. Please note that any payment after the deadline given, you are expected to get in touch with the Bureau, to issue a letter of renewal of offer before any further payment. <br /><br /> 6. You are also requested to please forward the following documents to this office soonest to facilitate issuance of your Certificate of Occupancy after payment of the requisite fees: <br /> <br/> i)&nbsp;&nbsp;Photocopy of all receipts issued for fees demanded above. <br /> ii)&nbsp;&nbsp;You are to obtain and submit duly completed application form for State Land. <br /> iii)&nbsp;&nbsp;Photocopy of Current Tax Clearance of two directors. <br /> iv)&nbsp;&nbsp;Photocopy of Ogun State Development Levy receipts from 2018 to 2021 of two directors. <br/> v)&nbsp;&nbsp;Photocopy of means of identification two (2) directors of the company. <br/> vi)&nbsp;&nbsp;Four (4) passport photograph for two (2) directors on white background. <br/> vii)&nbsp;&nbsp;Board resolution in respect of the subject transaction. <br/> viii)&nbsp;&nbsp;Photocopy of Memorandum and Articles of Association. <br/> ix)&nbsp;&nbsp;Photocopy of Certificate of Incorporation. <br /><br /> 7. Please note that you will be responsible for the payment of compensation for crops enumeration and any other unexhausted improvements on the land if any. <br /><br /> 8. In addition to (7) above, you will also be required to pay a minimum of Two Million Naira (N2,000,000.00) per hectare being gratuitous land compensation depending on the location of the subject property. <br /><br />9.  If the terms and conditions stated above are acceptable to you, please forward a letter of acceptance with evidence of payment receipts/tellers to this office on or before the next (60) days of your receiving this letter, failing which the offer would be deemed to have been rejected or lapsed. <br /><br /> 10. Many thanks.";

            var standardLetterDescriptions = assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} IN LAND SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()} {payerstate}.";

            var firstBodyWord = $"Refer to the above subject matter, please. <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b>&nbsp; has graciously considered your request and approved the {serviceSubHeader}";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);

            sb.AppendFormat(@$"<html>
                            <head>
                            </head>
                            <body style='width: 97%'><div style='text-align: left;' class='flex-container'>{payerFileNo}</div>");

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div><br>");


            if (!string.IsNullOrEmpty(salutationName))
            {
                sb.Append(@$"{salutationName} <br>");
            }
            if (!string.IsNullOrEmpty(assigneeName))
            {
                sb.Append(@$"{assigneeName} <br>");
                sb.Append(@"C/o");
            }

            sb.AppendFormat(@"<div class='row'>
                             <div class='column'> {0}<br> {1}</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{2} <br> Phone No.: {3}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{4}</q> <br> <span class='secondheader'>{5}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{6}</div> <br>
                            <div class='row'>
                            <div class='column'> <b>Terms</b> <br> Type of Use: <br> Size: <br> Terms of Grant: <br> Rent Revision Period <br> <br> <b>FEES</b></div>
                            <div class='column'>&nbsp;&nbsp; <br> {7} <br> {8} <br> {9} <br> {10}</div>
                        </div>"
                            , salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords, typeofUse, sizeOfLand, termsofGrant, rentRevisionPeriod);



            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();
                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }


            sb.Append(@"</table>");

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);

            sb.AppendFormat(@"{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/><br> <br><br><br><br>", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<br><br><div style='text-align: center; padding-top: 10px;'>_____________________________</div>
                                <p style='text-align: center;'> {0} <br> {1} <br> {2}</p>
                             </body>
                            </html>
                            ", signatoryName, signatoryPosition, secondSignatoryPosition);

            return sb.ToString();
        }
        public string GetRevalidationOfLandTitleHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var payerFileNo = string.Empty;
            var sizeOfLand = "1.791 Hectares";
            var termsofGrant = "92 years";
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            //var payerFileNo = "ODD/PL/C.30/94";
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower());
            var payerstate = assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "WITHOUT PREJUDICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var secondSignatoryPosition = "For: Special Adviser/Director General (Lands)";
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = assessmentDetailFirstDefault.ServiceHeader;
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var agencyName = assessmentDetailFirstDefault.AgencyName.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var agencyLogo = assessmentDetailFirstDefault.AgencyLogo;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            var offerShortName = "revalidation";
            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }
            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div style='text-align: justify;'><br /> 2. You are requested to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable on the {offerShortName} as evidence of acceptance of this offer. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS.</div>";
            //&nbsp;&nbsp;&nbsp;&nbsp;
            var LandText = $"3. The offer is also subject to other implied and express conditions to be stated on the Certificate of Occupancy (that will be issued in evidence of this grant) and the Land Use Act CAP L5 LFN 2004. <br /><br /> 4. That you (applicant) should notify the Bureau upon full payments of your land charges for further processing of your title documents within the possible time frame. <br /><br /> 5. Please note that any payment after the deadline given, you are expected to get in touch with the Bureau, to issue a letter of renewal of offer before any further payment. <br /><br /> 6. You are also requested to please forward the following documents to this office soonest to facilitate issuance of your Certificate of Occupancy after payment of the requisite fees: <br /> <br/> i)&nbsp;&nbsp;Photocopy of all receipts issued for fees demanded above. <br /> ii)&nbsp;&nbsp;Photocopy of Current Tax Clearance of two directors of the applicant. <br /> iii)&nbsp;&nbsp;Photocopy of Ogun State Development Levy receipts for 2018 to 2021 of the applicant. <br/> iv) &nbsp;Photocopy of means of identification of the applicant. <br/> v) &nbsp;Four (4) recent passport photograph of the applicant on white background. <br /><br />7. If the terms and conditions stated above are acceptable to you, please forward a letter of acceptance with evidence of payment receipts/tellers to this office on or before the next (60) days of your receiving this letter, failing which the offer would be deemed to have been rejected or lapsed. <br /><br />8. Many thanks.";

            var standardLetterDescriptions = assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} IN LAND SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()} <br> {payerstate}.";

            var firstBodyWord = $"Refer to the above subject matter, please. <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b> &nbsp; has graciously considered your request and approved the {serviceSubHeader}";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);

            sb.AppendFormat(@$"<html>
                            <head>
                            </head>
                            <body style='width: 97%'><div style='text-align: left;' class='flex-container'>{payerFileNo}</div>");

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div>");

            sb.AppendFormat(@"
                        <div class='row'>
                            <div class='column'> {0}<br> {1} <br> {2}</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{3} <br> Phone No.: {4}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{5}</q> <br> <span class='secondheader'>{6}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{7}</div> <br>
                            <div class='row'>
                            <div class='column'> <b>Terms</b> <br> Type of Use: <br> Size: <br> Terms of Grant: <br> Rent Revision Period <br> <br> <b>FEES</b></div>
                            <div class='column'>&nbsp;&nbsp; <br> {8} <br> {9} <br> {10} <br> {11}</div>
                        </div>"
                            , payerFileNo, salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords, typeofUse, sizeOfLand, termsofGrant, rentRevisionPeriod);


            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();

                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }


            sb.Append(@"</table>");

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);

            sb.AppendFormat(@"{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/><br> <br><br><br><br>", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<br><br><div style='text-align: center; padding-top: 10px;'>_____________________________</div>
                                <p style='text-align: center;'> {0} <br> {1} <br> {2}</p>
                             </body>
                            </html>
                            ", signatoryName, signatoryPosition, secondSignatoryPosition);

            return sb.ToString();
        }
        public string GetAccomodationHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var payerFileNo = string.Empty;
            var sizeOfLand = string.Empty;
            var termsofGrant = string.Empty;
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            //var payerFileNo = "ODD/PL/C.30/94";
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower());
            var payerstate = assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "WITHOUT PREJUDICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var agencyName = assessmentDetailFirstDefault.AgencyName.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var agencyLogo = assessmentDetailFirstDefault.AgencyLogo;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            var secondSignatoryPosition = "For: Special Adviser/Director General (Lands)";
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = assessmentDetailFirstDefault.ServiceHeader;
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var offerShortName = "accomodation";
            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }

            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div class='thirdheader' style='text-align: justify;'><br /> 2. You are requested to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable on the {offerShortName} as evidence of acceptance of this offer. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS.</div>";
            //&nbsp;&nbsp;&nbsp;&nbsp;
            var LandText = $"3. The offer is also subject to other implied and express conditions to be stated on the Certificate of Occupancy (that will be issued in evidence of this grant) and the Land Use Act CAP L5 LFN 2004. <br /><br /> 4. That you (applicant) should notify the Bureau upon full payments of your land charges for further processing of your title documents within the possible time frame. <br /><br /> 5. Please note that any payment after the deadline given, you are expected to get in touch with the Bureau, to issue a letter of renewal of offer before any further payment. <br /><br /> 6. You are also requested to please forward the following documents to this office soonest to facilitate issuance of your Certificate of Occupancy after payment of the requisite fees: <br /> <br/> i) &nbsp;Photocopy of all receipts issued for fees demanded above. <br /> ii) &nbsp;Photocopy of Current Tax Clearance of two directors. <br /> iii) &nbsp;Photocopy of Ogun State Development Levy receipts for 2018 to 2021 of two directors. <br/> iv) &nbsp;Board resolution in respect of the subject transaction. <br/> v) &nbsp;Photocopy of means of identification for two (2) directors of the company. <br/> vi) &nbsp;Three (3) passport photograph for two (2) directors on white background. <br /><br /> 7. If the terms and conditions stated above are acceptable to you, please forward a letter of acceptance with evidence of payment receipts/tellers to this office on or before the next (60) days of your receiving this letter, failing which the offer would be deemed to have been rejected or lapsed. <br /><br />8. Many thanks.";

            var standardLetterDescriptions = assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()} {payerstate}.";

            var firstBodyWord = $"Refer to the above subject matter, please. <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b>&nbsp; has graciously considered your request and approved the {serviceSubHeader}";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);

            sb.AppendFormat(@$"<html>
                            <head>
                            </head>
                            <body style='width: 97%'><div style='text-align: left;' class='flex-container'>{payerFileNo}</div>");

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div><br>");


            if (!string.IsNullOrEmpty(salutationName))
            {
                sb.Append(@$"{salutationName} <br>");
            }

            if (!string.IsNullOrEmpty(assigneeName))
            {
                sb.Append(@$"{assigneeName} <br>");
                sb.Append(@"C/o");
            }

            sb.AppendFormat(@"
                        <div class='row'>
                            <div class='column'> {0}<br> {1}</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{2} <br> Phone No.: {3}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{4}</q> <br> <span class='secondheader'>{5}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{6}</div> <br>
                            <div class='row'>
                            <div class='column'> <b>Terms</b> <br> Type of Use: <br> Size: <br> Terms of Grant: <br> Rent Revision Period <br> <br> <b>FEES</b></div>
                            <div class='column'>&nbsp;&nbsp; <br> {7} <br> {8} <br> {9} <br> {10}</div>
                        </div>"
                            , salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords, typeofUse, sizeOfLand, termsofGrant, rentRevisionPeriod);

            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();
                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }

            sb.Append(@"</table>");

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);

            sb.AppendFormat(@"{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/><br> <br><br><br><br>", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<br><br><div style='text-align: center; padding-top: 10px;'>_____________________________</div>
                                <p style='text-align: center;'> {0} <br> {1} <br> {2}</p>
                             </body>
                            </html>
                            ", signatoryName, signatoryPosition, secondSignatoryPosition);


            return sb.ToString();
        }
        public string GetRatificationOfLandTitleHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var payerFileNo = string.Empty;
            var sizeOfLand = string.Empty;
            var termsofGrant = string.Empty;
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            //var payerFileNo = "ODD/PL/C.30/94";
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower());
            var payerstate = assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "WITHOUT PREJUDICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var secondSignatoryPosition = "For: Special Adviser/Director General (Lands)";
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = assessmentDetailFirstDefault.ServiceHeader;
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var agencyName = assessmentDetailFirstDefault.AgencyName.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var agencyLogo = assessmentDetailFirstDefault.AgencyLogo;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            var offerShortName = "ratification";
            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }
            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div class='thirdheader' style='text-align: justify;'><br /> 2. You are requested to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable on the {offerShortName} as evidence of acceptance of this offer. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS.</div>";
            //&nbsp;&nbsp;&nbsp;&nbsp;
            var LandText = $"3. The offer is also subject to other implied and express conditions to be stated on the Certificate of Occupancy (that will be issued in evidence of this grant) and the Land Use Act CAP L5 LFN 2004. <br /><br /> 4. That you (applicant) should notify the Bureau upon full payments of your land charges for further processing of your title documents within the possible time frame. <br /><br /> 5. Please note that any payment after the deadline given, you are expected to get in touch with the Bureau, to issue a letter of renewal of offer before any further payment. <br /><br /> 6. Please note further that this approval is predicated on the assumption that all the documents you presented are with genuine proof of ownership and that all information you supplied us is/are correct.  <br /><br /> 7. The consideration of your request for ratification is without prejudice to pre-existing transactions entered into by you for which you remain solely responsible. <br /><br /> 8.  If the terms and conditions stated above are acceptable to you, please forward a letter of acceptance with evidence of payment receipts/tellers to this office on or before the next (60) days of your receiving this letter, failing which the offer would be deemed to have been rejected or lapsed. <br /><br />9. Many thanks.";

            var standardLetterDescriptions = assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} IN LAND SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()} {payerstate}.";

            var firstBodyWord = $"Refer to the above subject matter, please. <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b> &nbsp;has graciously considered your request and approved the {serviceSubHeader}";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);

            sb.AppendFormat(@$"<html>
                            <head>
                            </head>
                            <body style='width: 97%'><div style='text-align: left;' class='flex-container'>{payerFileNo}</div>");

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div>");


            if (!string.IsNullOrEmpty(salutationName))
            {
                sb.Append(@$"{salutationName} <br>");
            }

            if (!string.IsNullOrEmpty(assigneeName))
            {
                sb.Append(@$"{assigneeName} <br>");
                sb.Append(@"C/o");
            }
            sb.AppendFormat(@"<br><div class='row'>
                            <div class='column'> {0}<br> {1} </div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{2} <br> Phone No.: {3}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{4}</q> <br> <span class='secondheader'>{5}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{6}</div> <br>
                            <div class='row'>
                            <div class='column'> <b>Terms</b> <br> Type of Use: <br> Size: <br> Terms of Grant: <br> Rent Revision Period <br> <br> <b>FEES</b></div>
                            <div class='column'>&nbsp;&nbsp; <br> {7} <br> {8} <br> {9} <br> {10}</div>
                        </div>"
                            , salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords, typeofUse, sizeOfLand, termsofGrant, rentRevisionPeriod);


            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();

                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }


            sb.Append(@"</table>");

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);
            sb.AppendFormat(@"{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/><br> <br><br><br><br>", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<br><br><div style='text-align: center; padding-top: 10px;'>_____________________________</div>
                                <p style='text-align: center;'> {0} <br> {1} <br> {2}</p>
                             </body>
                            </html>
                            ", signatoryName, signatoryPosition, secondSignatoryPosition);


            return sb.ToString();
        }
        public string GetCertificateOfOccupancyHTMLString(List<AssessmentDTO> assessmentDetail, List<BillAdditionalInfoDto> additionalInfo, List<BillDetailsDto> billDetails, string howToPayUrl, string confirmDocUrl)
        {
            var howToPayBarcode = howToPayUrl;
            var confirmDocBarcode = confirmDocUrl;
            var assigneeName = string.Empty;
            var propertyLocation = "NOT PROVIDED AT THE ASSESSMENT STAGE";
            var salutationName = string.Empty;
            var payerFileNo = string.Empty;
            var sizeOfLand = string.Empty;
            var termsofGrant = string.Empty;
            var sb = new StringBuilder();
            if (!assessmentDetail.Any())
            {
                var sbNoRecord = new StringBuilder();
                sbNoRecord.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <br><br><br><br><br><br>
                                <div class='header'><h1>Attention!!!</h1></div>
                                    <div class='firstheader'>
                                   No Record to Display. Service does not required a standard letter. If otherwise kindly contact 
                                    the administrator. Thanks.
                                    </div>
                                ");

                sbNoRecord.Append(@"</body>
                        </html>");

                return sbNoRecord.ToString();
            }
            var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
            var amountInWords = NumberToWordsConverter.ConvertToWords((decimal)billDetails.Sum(x => x.TotalBillAmount));
            var salutation = assessmentDetailFirstDefault.Name.ToUpper();
            var payerAddress1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assessmentDetailFirstDefault.Address.ToLower()); //TitleCase
            var payerstate = assessmentDetailFirstDefault.Location.ToUpper();
            var telePhone = assessmentDetailFirstDefault.Telephone;
            var assDate = assessmentDetailFirstDefault.DateSubmitted.ToLongDateString();
            var grpAssRefNo = assessmentDetailFirstDefault.BaseAssnumber;
            var paymentCode = assessmentDetailFirstDefault.PaymentCode;
            var sumOfRebate = billDetails.Sum(x => x.RebateAmount);
            var sumOfAssAmountFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.BillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var totalSum = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.TotalBillAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var sumOfRebateFormated = Convert.ToDecimal(Convert.ToDecimal(billDetails.Sum(x => x.RebateAmount)).ToString(CultureInfo.InvariantCulture)).ToString("N");
            var firstHeader = "WITHOUT PREJUDICE";
            var signatoryName = assessmentDetailFirstDefault.SignatoryName;
            var signatoryPosition = assessmentDetailFirstDefault.SignatoryPosition;
            var secondSignatoryPosition = "For: Special Adviser/Director General (Lands)";
            var stateGorvenorsName = assessmentDetailFirstDefault.StateGovernorName;
            var serviceSubHeader = assessmentDetailFirstDefault.ServiceSubHeader;
            var serviceHeader = assessmentDetailFirstDefault.ServiceHeader;
            var typeofUse = assessmentDetailFirstDefault.CategoryName;
            var rentRevisionPeriod = assessmentDetailFirstDefault.RentRevisionPeriod;
            var agencyName = assessmentDetailFirstDefault.AgencyName.ToUpperInvariant();
            var agencyAddress1 = assessmentDetailFirstDefault.AgencyAddress1.ToUpperInvariant();
            var agencyAddress2 = assessmentDetailFirstDefault.AgencyAddress2.ToUpperInvariant();
            var agencyLogo = assessmentDetailFirstDefault.AgencyLogo;
            var agencySignature = assessmentDetailFirstDefault.AgencySignature;
            //var offerShortName = "the certificate";
            var assigneeNameInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.AssigneeOrPropertyOwner);
            var propertyLocationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.PropertyLocation);
            var salutationInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.Salutation);
            var payerFileNos = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.FileNo);
            var sizeOfLandInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.SizeOfLand);
            var termsofGrantInDb = BillHelper.GetAdditionalInfoByField(additionalInfo, AdditionalServiceDetailDefinitions.TermsOfGrant);
            var payerFieldValue = payerFileNos.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(payerFieldValue))
            {
                payerFileNo = payerFieldValue;
            }
            var assigneeFieldValue = assigneeNameInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(assigneeFieldValue))
            {
                assigneeName = assigneeFieldValue.ToUpperInvariant();
            }

            var propertyLocationValue = propertyLocationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(propertyLocationValue))
            {
                propertyLocation = propertyLocationValue;
            }

            var salutationValue = salutationInDb.FirstOrDefault()?.FieldValue.ObjectToString();
            if (!string.IsNullOrEmpty(salutationValue))
            {

            }
            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var Landcompulsory = $"<div class='thirdheader' style='text-align: justify;'><br /> 1. You are requested to pay the sum of &nbsp;<b>{amountInWords} &nbsp;Only (₦{totalSum}) </b> being the total sum payable as evidence of acceptance of this offer. The said amount is expected to be paid using the Payment Code stated above in this document via any of the following payment channels : <br /> <p><p/>a.)&nbsp;Ogun State Government Approved Bank(s) via eCashier, Remitta, PayDirect Platform. &nbsp;<br/>b.) Online via {_appsettings.PaymentOnlineWebsite}. &nbsp; <br/>c.) POS.</div>";

            var standardLetterDescriptions = assessmentDetailFirstDefault.StandardLetterDescriptions;

            var secondHeadingWordings = $"{serviceHeader} SITUATE, LYING AND BEING AT {propertyLocation.ToUpper()} {payerstate}.";

            var firstBodyWord = $"Refer to the above subject matter, please. <br> <br> I am directed to inform you that <b>{stateGorvenorsName}</b>&nbsp; has graciously considered your request and approved the {serviceSubHeader}";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(Landcompulsory);
            var remainingBodyWordings = LineBreakers.ShowLineBreaks(standardLetterDescriptions);
            var firstBodyWords = LineBreakers.ShowLineBreaks(firstBodyWord);

            sb.AppendFormat(@$"<html>
                            <head>
                            </head>
                            <body style='width: 97%'><div style='text-align: left;' class='flex-container'>{payerFileNo}</div>");

            sb.AppendFormat(@$"
            <table style='border:none;'>
              <tr style='border:none;'>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{confirmDocBarcode}' alt='{confirmDocBarcode}'/><br><span style='text-align: left;'>Confirm Document</span></td>
                <td style='border:none; text-align: left;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img class='imageLeft' src='{agencyLogo}' alt='{agencyLogo}' style='width: 160px; height: 150px'/></td>
                <td style='border:none; text-align: right;'><img class='imageRight' src='{howToPayBarcode}' alt='{howToPayBarcode}'/><br>
                <span style='text-align: right;'>How to Pay</span>
                </td>
              </tr>
            </table>
            ");

            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: larger'><b>{agencyName}</b></span><br>{agencyAddress1}<br>{agencyAddress2}</div><br>");
            sb.AppendFormat(@$"<div style='text-align: center;'><span style='font-size: xx-large'><b>PAYMENT CODE : {paymentCode}</b></span></div><br>");


            if (!string.IsNullOrEmpty(salutationName))
            {
                sb.Append(@$"{salutationName} <br>");
            }
            if (!string.IsNullOrEmpty(assigneeName))
            {
                sb.Append(@$"{assigneeName} <br>");
                sb.Append(@"C/o");
            }

            sb.AppendFormat(@"<div class='row'>
                             <div class='column'> {0}<br> {1}</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <div class='column'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</div>
                            <b><div class='column'>{2} <br> Phone No.: {3}</div></b>
                        </div>
                            <div class='firstheader'><h4><q>{4}</q> <br> <span class='secondheader'>{5}</span> </h4></div>
                            <div class='thirdheader' style='text-align: justify;'>{6}</div> <br>
                            <div class='row'>
                            <div class='column'> <b>Terms</b> <br> Type of Use: <br> Size: <br> Terms of Grant: <br> Rent Revision Period <br> <br> <b>FEES</b></div>
                            <div class='column'>&nbsp;&nbsp; <br> {7} <br> {8} <br> {9} <br> {10}</div>
                        </div>"
                            , salutation, payerAddress1, assDate, telePhone, firstHeader, secondHeadingWordings, firstBodyWords, typeofUse, sizeOfLand, termsofGrant, rentRevisionPeriod);

            if (sumOfRebate > 0)
            {
                var rebatePercentage = billDetails.Where(x => x.RebateAmount > 0).Select(x => x.RebatePercentage).FirstOrDefault();
                var showRebatePercent = string.Empty;
                bool showRebatePercentStatus = billDetails.Where(x => x.RebateAmount > 0 && x.ShowRebateAmount == false).Any();
                if (!showRebatePercentStatus)
                {
                    showRebatePercent = "%";
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>{0}{1} &nbsp;Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>", rebatePercentage, showRebatePercent);
                }
                else
                {
                    sb.AppendFormat(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                        <th align='right'>Rebate (₦)&nbsp;</th>  
                                        <th align='right'>Total (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");
                }

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                    <td align='right'>&nbsp;{2}&nbsp;</td>
                                    <td align='right'>&nbsp;{3}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssesementAmountFormatted, emp.RebateAmountFormatted, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}</td>
                            <td align='right'>{1}</td>
                            <td align='right'>{2}&nbsp;</td>
                            </tr></tfoot>", sumOfAssAmountFormated, sumOfRebateFormated, totalSum);
            }
            else
            {
                sb.Append(@"
                              <br> 
                                <table align='left' style='width: 99%; padding-bottom: 500px;'>
                                    <thead>
                                    <tr align='left'>
                                        <th align='left'>&nbsp;Payment Item(s)</th>
                                        <th align='right'>Amount (₦)&nbsp;</th>
                                    </tr>
                                       </thead>");

                foreach (var emp in billDetails)
                {
                    sb.AppendFormat(@"<tbody><tr align='left'>
                                    <td align='left'>&nbsp;{0}</td>
                                    <td align='right'>&nbsp;{1}&nbsp;</td>
                                  </tr></tbody>", emp.PaymentItemName, emp.AssessmentBalanceFormatted);
                }

                sb.AppendFormat(@"<tfoot>
                            <tr align='left'>
                            <td>Total</td>
                            <td align='right'>{0}&nbsp;</td>
                            </tr></tfoot>", totalSum);
            }


            sb.Append(@"</table>");

            sb.AppendFormat(@"&nbsp;&nbsp;&nbsp;<br><b>{0}</b>", LandcompulsoryHeader);

            sb.AppendFormat(@"{0}<div class='thirdheader' style='text-align: justify;'>{1}<div/><br> <br><br><br><br>", secondBodyWordings, remainingBodyWordings);

            sb.AppendFormat(@"<br><br><div style='text-align: center; padding-top: 10px;'>_____________________________</div>
                                <p style='text-align: center;'> {0} <br> {1} <br> {2}</p>
                             </body>
                            </html>
                            ", signatoryName, signatoryPosition, secondSignatoryPosition);

            return sb.ToString();
        }
        public async Task<List<AssessmentList>> GetAllApprovedAssessmentsAsync(DataSourceLoadOptions loadOptions, string agencyCode)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("@AgencyCode", agencyCode, DbType.String);

            var result = await Task.FromResult(_dapper.GetAll<AssessmentList>($"[dbo].[LoadDetailsAllAssessment]", dbparams, commandType: CommandType.StoredProcedure, _appsettings.DefaultConnection));

            return result.ToList();
        }
        public async Task<Response<List<AdditionalServiceDetailDto>>> GetAllAdditionalServiceDetailName()
        {
            var response = new Response<List<AdditionalServiceDetailDto>>();
            var request = await _context.AdditionalServiceDetail.ToListAsync();
            response.Message = "Sucessful";
            response.Succeeded = true;
            return response;
        }
        public string GetPaymentInstructionHTMLStringAsync()
        {
            var assigneeName = string.Empty;
            var sb = new StringBuilder();

            var firstHeader = "HOW TO PAY";
            var secondParagraph = "You can pay via any of the following payment channels:";
            var paymentChannels = $"You can pay via any of the following payment channels: <p></p> 1.&nbsp;{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_appsettings.StateName.ToLowerInvariant())} Government approved banks using the e-cashier, remita and paydirect &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;payment platforms.<br />2.&nbsp;&nbsp;Online or web payment using this url: <b>{_appsettings.PaymentOnlineWebsite}.</b> <br /> 3.&nbsp;&nbsp;POS at point of Service.<br /><br /> Please find below the steps involved in processing your payment through the above mentioned payment channels: <br />";

            var signatoryName = "";
            var signatoryPosition = "";
            var secondSignatoryPosition = "";

            var LandcompulsoryHeader = "OTHER SPECIAL CONDITIONS";
            var LandText = $"<br />1. {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_appsettings.StateName.ToLowerInvariant())} Government approved banks using the e-cashier, remita and paydirect payment platforms: <br><br> <b>STEPS:</b><br><br> (a) Complete the customized government revenue holding account deposit slip for payment stating clearly the <b>PAYMENT CODE</b> on top of the deposit slip.<br><br>(b) Give the completed deposit slip and the assessment or demand notice to the bank teller. <br><br> (c) Insist that the payment should only be processed using e-cashier or remita or paydirect payment platforms. Direct lodgment into the government revenue holding account is <b>NOT allowed.</b><br><br> (d) Provide answers to the questions asked by the bank teller. <br><br> (e) Hand over your cash or cheque to the bank teller. <br><br>	(f)	Insist and demand for your e-ticket after processing the payment through any of the above three (3) mentioned payment platforms. <br><br> (g) Take a copy of your bank acknowledged deposit slip with e-ticket to the agency to collect the government official revenue receipt. <br><br>2. Online or web payment using this url: <b>{_appsettings.PaymentOnlineWebsite}:</b> <br><br> <b>STEPS:</b><br><br> (a) Log on to {_appsettings.PaymentOnlineWebsite} <br><br>	(b)	Input the <b>PAYMENT CODE</b> on the assessment or demand notice in the space provided. <br><br>	(c)	Click <b>“Get details”</b> to see the assessment or demand notice raised. <br><br>	(d)	Complete the payer’s details i.e first name, last name, email address and telephone number.<br><br> (e) Input the amount you want to pay in the space provided. <br><br> (f) Select the payment methods either by bank transfer or ATM card.<br><br> (g) Click <b>“make payment”</b>. <br><br> (h) Complete the bank account details or ATM card details as requested to the end of the transaction. <br><br> (i) Print the payment acknowledgement receipt and check your email box for a permanent copy of the payment acknowledgement receipt.<br><br> (j) Take a copy of your payment acknowledgement receipt to the agency to collect the government official revenue receipt.<br><br> 3. POS at Point of Service. <br><br> <b>STEPS:</b><br><br> (a) Give the assessment or demand notice to the MDA POS revenue officer. <br><br> (b) Provide answers to the questions asked by the MDA POS revenue officer. <br><br> (c) Hand over your ATM card to the MDA POS revenue officer. <br><br> (d) Insist and demand for your e-ticket after processing the payment. <br><br> (e) Remember to collect your ATM card and demand notice from the MDA POS revenue officer. <br><br> (f) Take a copy of your POS acknowledged deposit slip with e-ticket to the agency to collect official government revenue receipt.";

            var secondBodyWordings = LineBreakers.ShowLineBreaks(LandText);
            sb.AppendFormat(@"<html>
                            <head>
                            </head>
                            <body style='width: 97%;'>
                            <div class='firstheader'><h4 style='font-size: xx-large;'><q>{0}</q></h4></div>"
                            , firstHeader);

            sb.AppendFormat(@"<div style='text-align: justify; font-size: x-large;'>&nbsp;&nbsp;&nbsp;<br>{0}</div>", paymentChannels);

            sb.AppendFormat(@"<div style='text-align: justify; font-size: x-large;'>{0}<br><br></div>
                                <div style='text-align: center font-size: x-large;; padding-top: 10px;'><br><br></div>
                                <p style='text-align: center; font-size: x-large;'> {1} <br> {2}</p>
                                </div>
                            </body>
                            </html>", secondBodyWordings, signatoryPosition, secondSignatoryPosition);

            return sb.ToString();
        }
    }
}

        

