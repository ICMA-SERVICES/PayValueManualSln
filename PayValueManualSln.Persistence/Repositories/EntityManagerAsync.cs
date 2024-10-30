using Microsoft.EntityFrameworkCore;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueV2.Domain.Entities.PayValue;
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
		public EntityMangerAsync(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Response<List<ServicesDto>>> GetServicesAsync()
		{
			var response = new Response<List<ServicesDto>>();
			try
			{
				var result = await _context.Services
				.Where(a => a.Name != null)
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
				.Where(a => a.Name != null)
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
				// Retrieve the assessment data by ID
				var assessment = await _context.Assessment.FindAsync(assessmentId);
				if (assessment == null)
				{
					response.Succeeded = false;
					response.Message = "Assessment not found.";
					return response;
				}

				// Create and populate BillInfo object
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
					// Add other fields as needed
				};

				// Add BillInfo to the database context
				await _context.BillInfo.AddAsync(billInfo);

				// Create and populate BillDetails object
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
					IsReversed = assessment.IsReversed,
					// Add other fields as needed
				};

				// Add BillDetails to the database context
				await _context.BillDetails.AddAsync(billDetails);

				// Save changes to the database
				await _context.SaveChangesAsync();

				response.Succeeded = true;
				response.Message = "Assessment data successfully inserted into BillInfo and BillDetails.";
			}
			catch (Exception ex)
			{
				response.Succeeded = false;
				response.Message = $"An error occurred: {ex.Message}";
				// Log the exception details as necessary
			}
				
			return response;
		}


	}
}
