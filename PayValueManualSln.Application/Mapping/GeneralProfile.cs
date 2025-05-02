using AutoMapper;
using PayValueManualSln.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Mapping
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<UpdatePayerRequest, UpdateIndividualPayerRequestDto>();
            CreateMap<UpdatePayerRequest, UpdateAgentRequestDto>();
            CreateMap<UpdatePayerRequest, PayerDetailsDto>();

        }
    }
}
