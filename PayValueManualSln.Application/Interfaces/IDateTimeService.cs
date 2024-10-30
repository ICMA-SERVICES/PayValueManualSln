using System;
using System.Collections.Generic;
using System.Text;

namespace PayValueManualSln.Application.Interfaces
{
	public interface IDateTimeService
	{
		DateTime NowUtc { get; }
	}
}
