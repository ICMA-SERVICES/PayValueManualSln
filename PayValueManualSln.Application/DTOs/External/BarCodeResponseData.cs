using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.DTOs.External
{
    public class BarCodeResponseData
    {
        public string qrImageUrl { get; set; }
        public string base64QrImage { get; set; }
    }

    public class BarCodeResponse
    {
        public bool succeeded { get; set; }
        public string message { get; set; }
        public List<string> errors { get; set; }
        public BarCodeResponseData data { get; set; }
    }

    public class BarCodeRequest
    {
        public string rawString { get; set; }
    }
}
