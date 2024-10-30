using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Wrappers
{
	public interface IRequestWrapper<TResponse> : IRequest<Response<TResponse>>
	{
	}

	public interface IHandlerWrapper<in TRequest, TResponse> : IRequestHandler<TRequest, Response<TResponse>>
		where TRequest : IRequestWrapper<TResponse>
	{
	}
}
