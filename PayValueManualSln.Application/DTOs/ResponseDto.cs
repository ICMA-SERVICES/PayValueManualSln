using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs
{
	public class ResponseDto
	{
		public bool Succeeded { get; set; }
		public string Message { get; set; }
		public object Data { get; set; } 

	
		public ResponseDto(bool succeeded = true, string message = "", object data = null)
		{
			Succeeded = succeeded;
			Message = message;
			Data = data;
		}

		
		public ResponseDto(string errorMessage)
		{
			Succeeded = false;
			Message = errorMessage;
		}
	}

}
