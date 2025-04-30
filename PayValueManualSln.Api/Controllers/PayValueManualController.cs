using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Entities;
using System.Xml.Linq;

namespace PayValueManualSln.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
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
			};
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
			};
		}
        [HttpGet("get-assessment-detail")]
        public async Task<IActionResult> GetAssessmentDetail(DataSourceLoadOptions loadOptions,string searchParameter)
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
        [HttpPost("Get-list-revenue")]
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
            var response = await _entityManager.CreateAssessment(request);
            return Ok(response);
        }
    }

}

