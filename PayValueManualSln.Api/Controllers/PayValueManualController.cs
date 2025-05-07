using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Entities;
using System.Xml.Linq;

namespace PayValueManualSln.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PayValueManualController : ControllerBase
    {
        private readonly IEntityManager _entityManager;
        public PayValueManualController(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        [HttpGet("get-service")]
        public async Task<IActionResult> GetServices(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<ServicesDto>();
            var response = await _entityManager.GetServicesAsync();
            if (response.Succeeded)
            {
                responseList = response.Data;

                loadOptions.PrimaryKey = new[] { $"Name" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = response });
            }
            else
            {
                return BadRequest(response);
            }
            ;
        }
        [HttpGet("get-revenue")]
        public async Task<IActionResult> GetRevenue(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<RevenueDto>();
            var response = await _entityManager.GetRevenueAsync();
            if (response.Succeeded)
            {
                responseList = response.Data;

                loadOptions.PrimaryKey = new[] { $"RevenueName" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = response });
            }
            else
            {
                return BadRequest(response);
            }
            ;
        }
        [HttpGet("get-assessment-detail")]
        public async Task<IActionResult> GetAssessmentDetail(DataSourceLoadOptions loadOptions, string searchParameter)
        {
            var responseList = new List<PayerCollectionDetail>();
            var searchResult = await _entityManager.GetAssessmentDetailAsync(searchParameter);
            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data.payerCollectionDetails;
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }

        [HttpPost("create-bill-from-assessment")]
        public async Task<IActionResult> CreateBillFromAssessment(int assessmentId, [FromBody] AssesmentDto requeset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = _entityManager.InsertAssessmentDataToBillTablesAsync(assessmentId);
            return Ok(result);
        }
        [HttpPost("approve-payer-detail")]
        public async Task<IActionResult> ApprovePayerDetail([FromBody] UpdatePayerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _entityManager.ApprovePayerDetailAsync(request);
            return Ok(result);
        }

        [HttpPost("send-payer-detail-to-admin")]
        public async Task<IActionResult> SendPayerDetailToAdmin([FromBody] UpdatePayerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _entityManager.SendPayerDetialToAdminAsync(request);
            return Ok(result);
        }
        [HttpGet("get-additional-service-detail")]
        public async Task<IActionResult> GetAdditionalServiceDetail(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<AdditionalServiceDetailDto>();
            var response = await _entityManager.GetAdditionalServiceDetail();
            if (response.Succeeded)
            {
                responseList = response.Data;
                loadOptions.PrimaryKey = new[] { $"AdditionalServiceDetailName" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = response });
            }
            else
            {
                return BadRequest(response);
            }

        }
        [Authorize(Roles = "Initiator")]
        [HttpGet("get-utin")]
        public async Task<IActionResult> GetUtin(string username, int id)
        {
            var result = await _entityManager.GenerateStinAsync(username, id);
            return Ok(result);
        }
        [HttpGet("GetPendingAssessmentAsync")]
        public async Task<IActionResult> GetPendingAssessmentAsync(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<PayerDetailsDto>();
            var searchResult = await _entityManager.GetPendingAssessmentAsync();

            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"taxPayerReferenceNumber" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }
        [HttpGet("GetPendingAssessmentByRequesterIdAsync")]
        public async Task<IActionResult> GetPendingAssessmentByRequesterIdAsync(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<PayerDetailsDto>();
            var searchResult = await _entityManager.GetPendingAssessmentByRequesterIdAsync();

            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"taxPayerReferenceNumber" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }

        [HttpGet("ViewPendingAssessment/{payerUtin}")]
        public async Task<IActionResult> ViewPendingAssessment([FromRoute] string payerUtin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.ViewPendingAssessment(payerUtin);
            return Ok(response);
        }

        //UPDATE RECORD
        [HttpPost("list-revenue")]
        public async Task<IActionResult> GetRevenueList([FromBody] GetRateRequestDto requestDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.GetRevenuesForAssessmentAsync(requestDto);
            return Ok(response);
        }

        [HttpPost("CreateAssesment")]
        public async Task<IActionResult> CreateAssesmentAsync([FromBody] CreateAssessmentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.CreateAssessment(request);
            return Ok(response);
        }
        [HttpPost("GenerateBaseNumber")]
        public async Task<IActionResult> GenerateBaseNumber([FromQuery] string merchantCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = _entityManager.GenerateBaseNumber(merchantCode);
            return Ok(response);
        }
        [HttpPost("MapServiceToTypes")]
        public async Task<IActionResult> MapServiceToTypes([FromBody] List<MapServiceToTypeRequestDto> request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = _entityManager.MapServiceToType(request);
            return Ok(response);
        }
        [HttpGet("GetAssessments")]
        public async Task<IActionResult> GetAssessmentsAsync(DataSourceLoadOptions loadOptions, AssessmentStatus assessmentStatus, int? year = null)
        {
            if (string.Equals(year.ToString(), "undefined", StringComparison.OrdinalIgnoreCase))
            {
                year = null;
            }
            var responseList = new List<BillInfoDto>();
            var searchResult = await _entityManager.GetAllAssessmentsAsync(year, assessmentStatus);
            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"BillId" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }
        [HttpGet("GetAssessmentPendingApprovalByUserId")]
        public async Task<IActionResult> GetAssessmentPendingApprovalByUserIdAsync(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<BillInfoDto>();
            var searchResult = await _entityManager.GetAssessmentPendingApprovalByUserIdAsync();
            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"BillId" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }
        [HttpPost("ApproveAssessment")]
        public async Task<IActionResult> ApproveAssessmentAsync([FromBody] AssessmentApprovalRequest request)
        {
            var response = await _entityManager.ApproveAssessmentAsync(request);
            return Ok(response);
        }

    }
}

