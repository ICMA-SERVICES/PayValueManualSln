using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Interfaces;

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

		}
}
