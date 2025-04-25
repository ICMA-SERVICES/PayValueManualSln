using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
    public interface IHttpClientHelperService
    {
        /// <summary>
        /// Http client handler - Delete
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="merchantCode"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        Task DeleteAsync(string uri, string authToken = "");
        /// <summary>
        /// Http client handler - Get
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="merchantCode"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>

        Task<T> GetAsync<T>(string uri, string authToken = "", string branchId = "", string MerchantCode = "", string PayerIdentity = "");
        /// <summary>
        /// Http client handler - Post. Specifiy the base data and the class to deserialize json into
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="merchantCode"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        Task<TR> PostAsync<T, TR>(string uri, T data, string authToken = "", string SecretKey = "", string MerchantCode = "");
        Task<byte[]> PostPdfConvertAsync<T>(string uri, T data);
        /// <summary>
        /// Http client handler - Post. Specifiy the base data
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="merchantCode"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string uri, T data, string authToken = "");
        /// <summary>
        /// Http client handler - Put. Specifiy the base data
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="merchantCode"></param>
        /// <param name="authToken"></param>
        Task<T> PutAsync<T>(string uri, T data, string authToken = "");
        Task<TR> PutAsync<T, TR>(string uri, T data, string authToken = "");
    }
}
