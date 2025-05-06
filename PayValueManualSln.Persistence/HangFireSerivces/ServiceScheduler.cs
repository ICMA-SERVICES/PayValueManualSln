using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayValueV2.Domain.Entities.PayValue;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Application.Wrappers;

namespace PayValueManualSln.Infrastructure.Persistence.HangFireSerivces
{
    public class ServiceScheduler
    {
        private readonly IEntityManager _entityManager;
        private readonly ApplicationDbContext _context;
        public ServiceScheduler(IEntityManager entityManager, ApplicationDbContext context)
        {
            _entityManager = entityManager;
            _context = context;
        }

        public async Task<MessageClass> GenerateExternalPaymentCodeAsync(string baseNumber, bool? updateBill = false)
        {
            var bc = new MessageClass();

            try
            {
                var response = await _entityManager.GenerateExternalPaymentCode(baseNumber, updateBill);
                bc.StatusMessage = response.StatusMessage;
                bc.StatusId = response.StatusId;
                var SerializeResponse = JsonConvert.SerializeObject(bc);
                Log.Information(SerializeResponse);
            }
            catch (Exception ex)
            {
                bc.StatusId = -1;
                bc.StatusMessage = ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message;
            }

            return bc;
        }

        public async Task<MessageClass> GenerateExternalPaymentCodeForPendingAssessmentAsync()
        {
            var bc = new MessageClass();

            try
            {
                var response = await _entityManager.GenerateExternalPaymentCodeForPendingAssessment();
                bc.StatusMessage = response.StatusMessage;
                bc.StatusId = response.StatusId;
                var SerializeResponses = JsonConvert.SerializeObject(bc);
                Log.Information(SerializeResponses);

            }
            catch (Exception ex)
            {
                bc.StatusId = -1;
                bc.StatusMessage = ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message;
            }

            return bc;
        }

        public async Task<MessageClass> WithDrawExternalPaymentCodeAsync(string formerBaseNumber)
        {
            var bc = new MessageClass();

            try
            {
                var response = await _entityManager.WithDrawExternalPaymentCodeAsync(formerBaseNumber);
                bc.StatusMessage = response.StatusMessage;
                bc.StatusId = response.StatusId;
                var SerializeResponse = JsonConvert.SerializeObject(bc);
                Log.Information(SerializeResponse);
            }
            catch (Exception ex)
            {
                bc.StatusId = -1;
                bc.StatusMessage = ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message;
            }

            return bc;
        }

        public async Task<Response<string>> SendAssessmentToRepository(string baseNumber = null)
        {
            var bc = new Response<string>();

            try
            {
                var response = await _entityManager.SendToAssessmentRepository(baseNumber);
                var SerializeResponse = JsonConvert.SerializeObject(response);
                Log.Information(SerializeResponse);
            }
            catch (Exception ex)
            {
                bc.Data = null;
                bc.Succeeded = false;
                bc.Message = ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message;
            }

            return bc;
        }
        
        public async Task<Response<string>> CalculateRenewalDate()
        {
            var response = new Response<string>();

            try
            {
                response = await _entityManager.CalculateRenewalDate();
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Succeeded = false;
                response.Message = ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message;
            }

            return response;
        }
        
        public async Task<Response<string>> RenewAssessment()
        {
            var response = new Response<string>();

            try
            {
                response = await _entityManager.RenewAssessments();
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.Succeeded = false;
                response.Message = ex.InnerException == null ? "Error occurred while processing your request" : ex.InnerException.Message;
            }

            return response;
        }

    }
}
