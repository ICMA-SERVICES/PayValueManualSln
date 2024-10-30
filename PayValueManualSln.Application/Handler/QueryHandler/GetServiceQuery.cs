using MediatR;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Handler.QueryHandler
{
	public class GetServiceQuery : IRequest<Response<List<ServicesDto>>>
	{
	}
	public class GetServiceQueryHandler : IRequestHandler<GetServiceQuery, Response<List<ServicesDto>>>
	{
		private readonly IEntityManager _entityManager;

		public GetServiceQueryHandler(IEntityManager entityManager)
		{
			_entityManager = entityManager;
		}
		public async Task<Response<List<ServicesDto>>> Handle(GetServiceQuery request, CancellationToken cancellationToken)
		{
			var response = await _entityManager.GetServicesAsync();
			return response;
		}
	}
}