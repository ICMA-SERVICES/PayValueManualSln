using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Helpers
{
    public static class AppWebExtension
    {
        public static string GetQueryStringValue<T>(this HttpRequest request, int position = 1)
        {
            var list = request.GetDisplayUrl();
            if (!list.Any()) return null;
            var value = list.Skip(position - 1).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(value.ToString())) return null;
            return value.ToString();
        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-') // replace URL unsafe characters with safe ones
              .Replace('/', '_') // replace URL unsafe characters with safe ones
              .Replace("=", ""); // no padding
        }

        public static string Base64UrlDecode(string input)
        {
            string incoming = input.Replace('_', '/').Replace('-', '+');
            switch (input.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(incoming);
            string originalText = Encoding.ASCII.GetString(bytes);

            return originalText;
        }

        //Decode
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        //Encode
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static TimeSpan CalculateCacheExpirationTime()
        {
            // Calculate the time when you want the cache to expire (24 hours from the current time)
            var nextDay = DateTime.Now.AddDays(1);

            // Calculate the time remaining until the next day
            var timeUntilNextDay = nextDay - DateTime.Now;
            return timeUntilNextDay;
        }
    }
}
