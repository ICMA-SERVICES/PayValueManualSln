using Newtonsoft.Json;
using PayValueManualSln.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PayValueManualSln.Shared.Services
{
    public class HttpClientHelperService : IHttpClientHelperService
    {

        public async Task<T> GetAsync<T>(string uri, string authToken, string param, string MerchantCode = "", string PayerIdentity = "")

        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken, param);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("MerchantCode", MerchantCode);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("PayerIdentity", PayerIdentity);

                string jsonResult = string.Empty;

                var responseMessage = await httpClient.GetAsync(uri);

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                else if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                 responseMessage.StatusCode == HttpStatusCode.Unauthorized ||
                 responseMessage.StatusCode == HttpStatusCode.NotFound ||
                 responseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;

                }
                else
                {
                    return default(T);
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return default(T);
            }
        }


        public async Task<T> PostAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                    responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return default(T);
                }

                return default(T);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return default(T);
            }
        }

        public async Task<TR> PostAsync<T, TR>(string uri, T data, string authToken = "", string SecretKey = "", string MerchantCode = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                if (!String.IsNullOrEmpty(SecretKey) && !String.IsNullOrEmpty(MerchantCode))
                {
                    content.Headers.Add("SecretKey", SecretKey);
                    content.Headers.Add("MerchantCode", MerchantCode);
                }

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));
                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var st = JsonConvert.SerializeObject(data);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return json;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                    responseMessage.StatusCode == HttpStatusCode.BadRequest ||
                    responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return json;
                }

                var jsonResp = JsonConvert.DeserializeObject<TR>(jsonResult);
                return jsonResp;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return default;
            }
        }

        public async Task<byte[]> PostPdfConvertAsync<T>(string uri, T data)
        {
            try
            {
                HttpClient httpClient = CreateHttpClient();

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));
                if (responseMessage.IsSuccessStatusCode)
                {
                    // Read the response content as a byte array (PDF file)
                    var pdfContent = await responseMessage.Content.ReadAsByteArrayAsync();
                    return pdfContent;
                }

                return Array.Empty<byte>();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return default;
            }
        }

        public async Task<T> PutAsync<T>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PutAsync(uri, content));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                    responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return default(T);
                }

                return default(T);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return default(T);
            }
        }
        public async Task<TR> PutAsync<T, TR>(string uri, T data, string authToken = "")
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.PutAsync(uri, content));
                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return json;
                }
                if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                                   responseMessage.StatusCode == HttpStatusCode.BadRequest ||
                                   responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return json;
                }

                var jsonResp = JsonConvert.DeserializeObject<TR>(jsonResult);
                return jsonResp;

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                return default;
            }
        }

        public async Task DeleteAsync(string uri, string authToken = "")
        {
            HttpClient httpClient = CreateHttpClient(authToken);
            await httpClient.DeleteAsync(uri);
        }

        private HttpClient CreateHttpClient(string authToken = "", string param = "")
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var httpClient = new HttpClient(clientHandler);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }
            if (!string.IsNullOrEmpty(param))
            {
                httpClient.DefaultRequestHeaders.Add("browserUrl", param);
            }

            return httpClient;
        }
    }
}
