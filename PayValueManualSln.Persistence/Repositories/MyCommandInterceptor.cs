using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Persistence.Repositories
{
	public class MyCommandInterceptor : DbCommandInterceptor
	{
		public override InterceptionResult<int> NonQueryExecuting(
			DbCommand command,
			CommandEventData eventData,
			InterceptionResult<int> result)
		{
			// Intercept non-query commands (e.g., INSERT, UPDATE, DELETE)
			// Implement custom logic here
			Console.WriteLine($"Executing command: {command.CommandText}");

			// Continue with the operation
			return result;
		}
	}
}
