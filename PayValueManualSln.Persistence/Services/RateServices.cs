using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PayValueManualSln.Application.DTOs.RateDto;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Shared.DapperServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Persistence.Services
{

    public class RateServices
    {
        private readonly IAuthenticatedUserService _authenticatedUser;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IDapper _dapper;
        private readonly string _agencycode;
        public RateServices(IAuthenticatedUserService authenticatedUser,
                               ApplicationDbContext context,
                               IMapper mapper,
                               IDapper dapper)
        {
            _authenticatedUser = authenticatedUser;
            _context = context;
            _mapper = mapper;
            _dapper = dapper;
            _agencycode = "400070021110000";
            //_agencycode = _authenticatedUser.AgencyCode;

        }

        //public async Task<long> CheckIsFormularRequired(long revenueId)
        //{
        //    var check = await _context.ServiceRevenue.FirstOrDefaultAsync(x => x.Id == revenueId && x.IsFormulaeRequired == true);
        //    if (check == null)
        //        return 0;
        //    else
        //        return (long)check.ServiceMethodId;
        //}

        public async Task<bool> CheckIsForAllZones(long? serviceId)
        {
            return await _context.Service.AnyAsync(x => x.Id == serviceId && x.IsForAllZones == true);
        }


        public async Task<List<Rate>> GetRate(long? serviceId, long? locationId, long? zoneId, bool isForAllZones, long? typeId)
        {
            return (await _context.Rate.Where(x => x.ServiceId == serviceId && x.TypeId == typeId && x.IsApproved == true)
                .ToListAsync());

        }

        public async Task<List<ServiceRevenue>> GetRevenues(long? serviceId)
        {
            var revenues = await _context.ServiceRevenue.Where(x => x.ServiceId == serviceId).ToListAsync();
            return revenues;
        }

        public async Task<string> GetFormularName(long servicemethodId)
        {
            var methodName = await _context.ServiceMethod.FirstOrDefaultAsync(x => x.Id == servicemethodId);
            if (methodName == null)
            {
                return null;
            }
            else
            {
                return methodName.Name;
            }
        }

        public async Task<decimal> CalculateRevenue(CalculationParameters calculationParameters)
        {
            dynamic templateValue = new ValueTemplateForLocation();
            if (calculationParameters.IsServiceForAllLocation == true || calculationParameters.LocationId == null || calculationParameters.LocationId <= 0)
            {
                templateValue = await _context.ValueTemplateForLocation.Select(x => new { x.Id, x.Amount, x.LocationId, x.AgencyCode, x.TypeId }).FirstOrDefaultAsync(x => x.TypeId == calculationParameters.TypeId && x.AgencyCode == _agencycode);
            }
            else
            {
                templateValue = await _context.ValueTemplateForLocation.Select(x => new { x.Id, x.Amount, x.LocationId, x.AgencyCode, x.TypeId }).FirstOrDefaultAsync(x => x.LocationId == calculationParameters.LocationId && x.TypeId == calculationParameters.TypeId && x.AgencyCode == _agencycode);
            }


            if (templateValue != null)
            {
                calculationParameters.AmountForTemplateValue = templateValue.Amount;
            }

            //split rate amount to see if it is a special calculation
            var splitRateAmount = calculationParameters.RateAmount.Split("*");
            if (splitRateAmount.Length > 1)
            {
                decimal multiply = Decimal.Parse(splitRateAmount[0]) * Decimal.Parse(splitRateAmount[1]);
                calculationParameters.CalculatedRateAmount = multiply;
            }
            else
            {
                calculationParameters.CalculatedRateAmount = Decimal.Parse(calculationParameters.RateAmount);
            }
            //get the formular for calculation
            var methodName = await _context.ServiceMethod.FirstOrDefaultAsync(x => x.Id == calculationParameters.ServiceMethodId);
            if (methodName == null)
            {
                return 0;
            }
            else
            {
                try
                {
                    dynamic calulateRevenue = methodName.Formular;
                    var setupInfo = new ServiceMethodSetup();
                    if (methodName.ServiceMethodCode == "MULTX2")
                    {
                        setupInfo = await _context.ServiceMethodSetups.FirstOrDefaultAsync(x => x.ServiceId == calculationParameters.ServiceId && x.TypeId == calculationParameters.TypeId);
                        if (setupInfo != null)
                        {
                            if ((decimal)calculationParameters.LandSize <= setupInfo.MinimumLandSize)
                            {
                                return setupInfo.MinimumAmount;
                            }
                        }
                    }
                    if (methodName.ServiceMethodCode == "RANGE")
                    {
                        calculationParameters.LandSize = Math.Ceiling((double)calculationParameters.LandSize);
                        var range = await _context.Range
                            .FirstOrDefaultAsync(r => r.RateId == calculationParameters.RateId && (decimal)calculationParameters.LandSize >= r.MinimumValue && (decimal)calculationParameters.LandSize <= r.MaximumValue && r.AgencyCode == _agencycode);

                        if (range != null && range.IsMultiplyRange == true)
                        {
                            var input = await _context.Input.FirstOrDefaultAsync(x => x.Id == calculationParameters.InputDefinitionId);
                            decimal? rangeAmount = 0;
                            if (input.InputName.Trim().ToLower() == InputDefinitionEnum.Value.ToDescription().Trim().ToLower())
                            {
                                rangeAmount = range.RateValue * calculationParameters.Value;
                            }
                            else if (input.InputName.Trim().ToLower() == InputDefinitionEnum.Page.ToDescription().Trim().ToLower())
                            {
                                rangeAmount = range.RateValue * calculationParameters.Pages;
                            }
                            else
                            {
                                rangeAmount = range.RateValue * (decimal)calculationParameters.LandSize;
                            }
                            return (decimal)rangeAmount;
                        }
                        return range != null ? (decimal)range.RateValue : 0;
                    }

                    if (methodName.ServiceMethodCode == "PERCENT")
                    {
                        var input = await _context.Input.FirstOrDefaultAsync(x => x.Id == calculationParameters.InputDefinitionId);
                        if (input.InputName.Trim().ToLower() == InputDefinitionEnum.Value.ToDescription().Trim().ToLower())
                        {
                            calulateRevenue = calulateRevenue.Replace("InputVariable", calculationParameters.Value.ToString());
                        }
                        else if (input.InputName.Trim().ToLower() == InputDefinitionEnum.Page.ToDescription().Trim().ToLower())
                        {
                            calulateRevenue = calulateRevenue.Replace("InputVariable", calculationParameters.Pages.ToString());
                        }
                        else
                        {
                            calulateRevenue = calulateRevenue.Replace("InputVariable", calculationParameters.LandSize.ToString());
                        }
                    }

                    calulateRevenue = calulateRevenue.Replace("MinimumAmount", setupInfo == null ? "" : setupInfo.MinimumAmount.ToString());
                    calulateRevenue = calulateRevenue.Replace("ActualRate", setupInfo == null ? "" : setupInfo.ActualRate.ToString());
                    calulateRevenue = calulateRevenue.Replace("SizeOfLand", setupInfo == null ? "" : setupInfo.MinimumLandSize.ToString());
                    calulateRevenue = calulateRevenue.Replace("SuppliedValue", calculationParameters.LandSize.ToString());
                    calulateRevenue = calulateRevenue.Replace("LandSize", string.IsNullOrEmpty(calculationParameters.LandSize.ToString()) ? 0.ToString() : calculationParameters.LandSize.ToString());
                    calulateRevenue = calulateRevenue.Replace("CalculatedRateAmount", string.IsNullOrEmpty(calculationParameters.CalculatedRateAmount.ToString()) ? 0.ToString() : calculationParameters.CalculatedRateAmount.ToString());
                    calulateRevenue = calulateRevenue.Replace("AmountForTemplateValue", string.IsNullOrEmpty(calculationParameters.AmountForTemplateValue.ToString()) ? 0.ToString() : calculationParameters.AmountForTemplateValue.ToString());
                    calulateRevenue = decimal.Parse(new DataTable().Compute(calulateRevenue, null).ToString());
                    return calulateRevenue;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
    // Fix for CS1061: Add an extension method for InputDefinitionEnum to provide the ToDescription functionality.
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                .Cast<System.ComponentModel.DescriptionAttribute>()
                .FirstOrDefault();
            return attribute?.Description ?? value.ToString();
        }
    }

}

