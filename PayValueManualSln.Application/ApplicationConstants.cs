using PayValueManualSln.Application.Dtos.Shared;
using PayValueManualSln.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application
{
    public class ApplicationConstants
    {
        public static string SuccessResponseCode = "00";
        public static string FailureResponse = "-1";
        public static int SuccessStatusCode = 200;
        public static int NotFoundStatusCode = 404;
        public static int NotAuthenticatedStatusCode = 401;
        public static int BadRequestStatusCode = 400;


        public static string Sp_MenuSetup = "Sp_MenuSetup";
        public static string SP_SummaryReport = "SP_SummaryReport";

        public static string GenerateBarcodeMethod = "/barcode/api/v1/QrCoder/encode-string";
        public static string CreateBillingUrl = "/api/v1/Bill";
        public static string UpdateBillingUrl = "/api/v1/Bill/Update-bill";
        public static string WithdrawBillingUrl = "/api/v1/Bill/withdraw-bill";

        public ApplicationConstants()
        {
        }
        public static RepositoryResponse RepositoryFailed()
        {
            var response = new RepositoryResponse
            {
                Succeeded = false,
                Message = "Failed",
                StatusCode = 0
            };
            return response;
        }
        public static RepositoryResponse RepositoryExists()
        {
            var response = new RepositoryResponse
            {
                Succeeded = false,
                Message = "already exists",
                StatusCode = -1
            };
            return response;
        }

        public static RepositoryResponse RepositorySuccess()
        {
            var response = new RepositoryResponse
            {
                Succeeded = true,
                Message = "Done",
                StatusCode = 1
            };
            return response;
        }



        public static Response<string> SuccessMessage(string message, string returnString = null)
        {
            var response = new Response<string>
            {
                Data = null,
                Message = message,
                ResponseCode = "00",
                StatusCode = 200,
                Succeeded = true,
            };
            return response;
        }
        public static Response<T> SuccessMessage<T>(T obj, string message)
        {
            var response = new Response<T>
            {
                Data = obj,
                Message = message,
                ResponseCode = "00",
                StatusCode = 200,
                Succeeded = true,
            };
            return response;
        }

        public static Response<T> NullResponse<T>(T obj, string message)
        {
            var response = new Response<T>
            {
                Data = obj,
                Message = message,
                ResponseCode = "00",
                StatusCode = 200,
                Succeeded = true,
            };
            return response;
        }

        public static Response<string> FailureMessage(string message)
        {
            var response = new Response<string>
            {
                Data = null,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 400,
                Succeeded = false,
            };
            return response;
        }

        public static Response<string> UnauthorizedMessage(string message)
        {
            var response = new Response<string>
            {
                Data = null,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 401,
                Succeeded = false,
            };
            return response;
        }

        public static Response<string> NotFoundMessage(string message)
        {
            var response = new Response<string>
            {
                Data = null,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 404,
                Succeeded = false,
            };
            return response;
        }
        public static Response<T> FailureMessage<T>(T obj, string message)
        {
            var response = new Response<T>
            {
                Data = obj,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 400,
                Succeeded = false,
            };
            return response;
        }

        public static Response<T> NotFoundMessage<T>(T obj, string message)
        {
            var response = new Response<T>
            {
                Data = obj,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 404,
                Succeeded = false,
            };
            return response;
        }
        public static Response<string> AlreadyExistMessage(string message)
        {
            var response = new Response<string>
            {
                Data = null,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 409,
                Succeeded = false,
            };
            return response;
        }

        public static Response<T> AlreadyExistMessage<T>(T obj, string message)
        {
            var response = new Response<T>
            {
                Data = obj,
                Message = message,
                ResponseCode = "-1",
                StatusCode = 409,
                Succeeded = false,
            };
            return response;
        }
    }
}
