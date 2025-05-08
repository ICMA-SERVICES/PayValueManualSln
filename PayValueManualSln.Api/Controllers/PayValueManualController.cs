using DevExtreme.AspNet.Data;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.DTOs.External;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Common;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Domain.Entities.Settings;
using Polly.Utilities;
using Serilog;
using System;
using System.Xml.Linq;

namespace PayValueManualSln.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PayValueManualController : ControllerBase
    {
        private readonly IEntityManager _entityManager;
        private readonly Appsettings _appSettings;
        private IConverter _converter;
        public PayValueManualController(IEntityManager entityManager, IConverter converter, IOptions<Appsettings> appSettings)
        {
            _entityManager = entityManager;
            _appSettings = appSettings.Value;
            _converter = converter;

        }
        [HttpGet("get-service")]
        public async Task<IActionResult> GetServices(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<ServicesDto>();
            var response = await _entityManager.GetServicesAsync();
            if (response.Succeeded)
            {
                responseList = response.Data;

                loadOptions.PrimaryKey = new[] { $"Name" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = response });
            }
            else
            {
                return BadRequest(response);
            }
            ;
        }
        [HttpGet("get-revenue")]
        public async Task<IActionResult> GetRevenue(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<RevenueDto>();
            var response = await _entityManager.GetRevenueAsync();
            if (response.Succeeded)
            {
                responseList = response.Data;

                loadOptions.PrimaryKey = new[] { $"RevenueName" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = response });
            }
            else
            {
                return BadRequest(response);
            }
            ;
        }
        [HttpGet("get-assessment-detail")]
        public async Task<IActionResult> GetAssessmentDetail(DataSourceLoadOptions loadOptions, string searchParameter)
        {
            var responseList = new List<PayerCollectionDetail>();
            var searchResult = await _entityManager.GetAssessmentDetailAsync(searchParameter);
            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data.payerCollectionDetails;
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }

        [HttpPost("create-bill-from-assessment")]
        public async Task<IActionResult> CreateBillFromAssessment(int assessmentId, [FromBody] AssesmentDto requeset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = _entityManager.InsertAssessmentDataToBillTablesAsync(assessmentId);
            return Ok(result);
        }
        [HttpPost("approve-payer-detail")]
        public async Task<IActionResult> ApprovePayerDetail([FromBody] UpdatePayerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _entityManager.ApprovePayerDetailAsync(request);
            return Ok(result);
        }

        [HttpPost("send-payer-detail-to-admin")]
        public async Task<IActionResult> SendPayerDetailToAdmin([FromBody] UpdatePayerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _entityManager.SendPayerDetialToAdminAsync(request);
            return Ok(result);
        }
        [HttpGet("get-additional-service-detail")]
        public async Task<IActionResult> GetAdditionalServiceDetail(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<AdditionalServiceDetailDto>();
            var response = await _entityManager.GetAdditionalServiceDetail();
            if (response.Succeeded)
            {
                responseList = response.Data;
                loadOptions.PrimaryKey = new[] { $"AdditionalServiceDetailName" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = response });
            }
            else
            {
                return BadRequest(response);
            }

        }
        [Authorize(Roles = "Initiator")]
        [HttpGet("get-utin")]
        public async Task<IActionResult> GetUtin(string username, int id)
        {
            var result = await _entityManager.GenerateStinAsync(username, id);
            return Ok(result);
        }
        [HttpGet("GetPendingAssessmentAsync")]
        public async Task<IActionResult> GetPendingAssessmentAsync(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<PayerDetailsDto>();
            var searchResult = await _entityManager.GetPendingAssessmentAsync();

            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"taxPayerReferenceNumber" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }
        [HttpGet("GetPendingAssessmentByRequesterIdAsync")]
        public async Task<IActionResult> GetPendingAssessmentByRequesterIdAsync(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<PayerDetailsDto>();
            var searchResult = await _entityManager.GetPendingAssessmentByRequesterIdAsync();

            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"taxPayerReferenceNumber" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }

        [HttpGet("ViewPendingAssessment/{payerUtin}")]
        public async Task<IActionResult> ViewPendingAssessment([FromRoute] string payerUtin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.ViewPendingAssessment(payerUtin);
            return Ok(response);
        }

        //UPDATE RECORD
        [HttpPost("list-revenue")]
        public async Task<IActionResult> GetRevenueList([FromBody] GetRateRequestDto requestDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.GetRevenuesForAssessmentAsync(requestDto);
            return Ok(response);
        }

        [HttpPost("CreateAssesment")]
        public async Task<IActionResult> CreateAssesmentAsync([FromBody] CreateAssessmentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.CreateAssessment(request);
            return Ok(response);
        }
        [HttpPost("GenerateBaseNumber")]
        public async Task<IActionResult> GenerateBaseNumber([FromQuery] string merchantCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = _entityManager.GenerateBaseNumber(merchantCode);
            return Ok(response);
        }
        [HttpPost("MapServiceToTypes")]
        public async Task<IActionResult> MapServiceToTypes([FromBody] List<MapServiceToTypeRequestDto> request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = _entityManager.MapServiceToType(request);
            return Ok(response);
        }
        [HttpGet("GetAssessments")]
        public async Task<IActionResult> GetAssessmentsAsync(DataSourceLoadOptions loadOptions, AssessmentStatus assessmentStatus, int? year = null)
        {
            if (string.Equals(year.ToString(), "undefined", StringComparison.OrdinalIgnoreCase))
            {
                year = null;
            }
            var responseList = new List<BillInfoDto>();
            var searchResult = await _entityManager.GetAllAssessmentsAsync(year, assessmentStatus);
            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"BillId" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }
        [HttpGet("GetAssessmentPendingApprovalByUserId")]
        public async Task<IActionResult> GetAssessmentPendingApprovalByUserIdAsync(DataSourceLoadOptions loadOptions)
        {
            var responseList = new List<BillInfoDto>();
            var searchResult = await _entityManager.GetAssessmentPendingApprovalByUserIdAsync();
            if (searchResult.Succeeded)
            {
                responseList = searchResult.Data;

                loadOptions.PrimaryKey = new[] { $"BillId" };
                var test = DataSourceLoader.Load(responseList, loadOptions);
                if (test.data != null)
                {
                    return Ok(test);
                }
                return Ok(new { ResonseList = test, SearchResult = searchResult });
            }
            else
            {
                return BadRequest(searchResult);
            }
        }
        [HttpPost("ApproveAssessment")]
        public async Task<IActionResult> ApproveAssessmentAsync([FromBody] AssessmentApprovalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.ApproveAssessmentAsync(request);
            return Ok(response);
        }
        [HttpGet("GetAssessmentsById")]
        public async Task<IActionResult> GetAssessmentsByIdAsync(long billId)
        {
            var response = await _entityManager.GetAssessmentsByIdAsync(billId);
            return Ok(response);
        }
        [HttpGet("GenerateExternalPaymentCode")]
        public async Task<IActionResult> GenerateExternalPaymentCode(string baseNumber, bool? updateBill = false)
        {
            var response = await _entityManager.GenerateExternalPaymentCode(baseNumber, updateBill);
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpPost("UpdateBaseNumber")]
        public async Task<IActionResult> UpdateBaseNumber(long billId, string baseNumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.UpdateBaseNumber(billId, baseNumber);
            return Ok(response);
        }

        [HttpPost("GetDepositAmount")]
        public async Task<IActionResult> GetDepositAmountAsync([FromBody] List<GetDepositRequestDto> request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _entityManager.GetDepositAmount(request);
            return Ok(response);
        }
        [HttpGet("assessmentLetter")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGeneratedAssessmentInPdfDocumentOnBrowser()
        {
            var resultTemplate = string.Empty;
            var assRefNo = string.Empty;
            var agencyCode = string.Empty;
            var confirmBarcode = string.Empty;
            var howToPayBarcode = string.Empty;
            byte[] file;
            try
            {
                //https://localhost:44342/api/Payvalue/assessmentLetter?&T0dTUz4zN=UFZPRzIxOVRUMDQ1LzQwMDA3MDAyMTExMDAwMA
                //https://localhost:44342/api/Payvalue/assessmentLetter?T0dTUz4zN=UFZPRzIxMEFBMDMx
                var encodedString = AppWebExtension.Base64UrlEncode("PVOG219TT045/400070021110000");
                string sentString = HttpContext.Request.Query["T0dTUz4zN"];
                if (sentString.Length < 0)
                {
                    resultTemplate = _entityManager.EmptyPdf();
                }
                else
                {
                    var decodedString = AppWebExtension.Base64UrlDecode(sentString.ObjectToString());
                    var stringSplit = decodedString.Split("/");
                    if (stringSplit.Length != 2)
                    {
                        var msg = "Invalid Information";
                        resultTemplate = _entityManager.EmptyPdf();
                    }

                    assRefNo = stringSplit[0];
                    agencyCode = stringSplit[1];
                    //var formulatedString = assRefNo + "/" + agencyCode;
                    //var encodeAssessmentRefNo = AppWebExtension.Base64UrlEncode(formulatedString.TrimEnd().ObjectToString());
                    confirmBarcode = $"{_appSettings.ServiceBaseUrl}/api/Payvalue/assessmentLetter?&T0dTUz4zN={sentString}";
                    howToPayBarcode = $"{_appSettings.ServiceBaseUrl}/api/Payvalue/paymentInstructions";
                }

                var sentPowerByHeaders = HttpContext.Request.Headers["powerBy"].ToString();
                var sentdocumentTitleHeaders = HttpContext.Request.Headers["documentTitle"].ToString();
                var senturlHeaders = HttpContext.Request.Headers["url"].ToString();
                var sentmerchantCodeHeaders = HttpContext.Request.Headers["merchantCode"].ToString();
                var sentserviceIdHeaders = HttpContext.Request.Headers["serviceId"].ToString();
                var sentserviceHeadingHeaders = HttpContext.Request.Headers["serviceHeading"].ToString();
                var sentPowerByHeadersDesirialized = JsonConvert.DeserializeObject<string>(sentPowerByHeaders);
                var sentdocumentTitleHeadersDesirialized = JsonConvert.DeserializeObject<string>(sentdocumentTitleHeaders);
                var senturlHeadersDesirialized = JsonConvert.DeserializeObject<string>(senturlHeaders);
                var sentmerchantCodeHeadersDesirialized = JsonConvert.DeserializeObject<string>(sentmerchantCodeHeaders);
                var sentserviceIdHeadersDesirialized = JsonConvert.DeserializeObject<string>(sentserviceIdHeaders);
                var sentserviceHeadingDesirialized = JsonConvert.DeserializeObject<string>(sentserviceHeadingHeaders);

                var sentDocumentTitle = "PDF Report"; // sentdocumentTitleHeadersDesirialized;
                var sentPowerBy = "Powered By" + " " + " " + "http://icmaservices.com"; //sentPowerByHeaders
                var sentUrl = "https://payment.deltabir.com/";

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 2 },
                    DocumentTitle = sentDocumentTitle,
                };

                var assessmentDetail = new List<AssessmentsDto>();
                var additionalInfo = new List<BillAdditionalInfoDto>();
                var billDetails = new List<BillDetailsDto>();
                assessmentDetail = await _entityManager.GetAllApprovedAssessmentNotices(agencyCode, assRefNo).ConfigureAwait(false);
                if (assessmentDetail.Any())
                {
                    var barCodeRequest = new BarCodeRequest
                    {
                        rawString = confirmBarcode
                    };

                    var confirmDocUrl = await _entityManager.GenerateConfirmDocBarcode(confirmBarcode);
                    var howToPayUrl = await _entityManager.GenerateBarcode(howToPayBarcode);

                    confirmDocUrl = string.IsNullOrEmpty(confirmDocUrl) || string.IsNullOrWhiteSpace(confirmDocUrl) ? howToPayUrl : confirmDocUrl;

                    var billInfoGuid = assessmentDetail.FirstOrDefault().BillInfoGuid;
                    additionalInfo = await _entityManager.GetAllAdditionalInfo(billInfoGuid);
                    billDetails = await _entityManager.GetBillDetail(billInfoGuid);

                    var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
                    resultTemplate = _entityManager.GetAssessmentsNoticeHtmlString(assessmentDetail, additionalInfo, billDetails, howToPayUrl, confirmDocUrl);
                }
                else
                {
                    resultTemplate = _entityManager.AssessmentEmptyPdf();
                }

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = resultTemplate, // TemplateGenerator.GetHTMLString(),//USE THIS PROPERTY TO GENERATE PDF CONTENT FROM AN HTML PAGE
                    //Page = sentUrl, //USE THIS PROPERTY TO GENERATE PDF CONTENT FROM FROM A SENT URL PAGE
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css"), EnableJavascript = true, enablePlugins = true },
                    //HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    //FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = sentPowerBy /*"Report Footer"*/ }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                file = _converter.Convert(pdf);  //Showing a PDF Document in a Browser //IF WE dont USE Out PROPERTY IN THE GlobalSettings CLASS, THIS IS ENOUGH FOR CONVERSION
                return File(file, "application/pdf");  //Showing a PDF Document in a Browser  //Note to use both Enabling Download Mode and Showing a PDF Document in a Browser, you must comment Out in globalSettings above.

            }
            catch (Exception ex)
            {
                Log.Error($"An Exception Error as occurred in an Application Url ({nameof(OpenGeneratedAssessmentPdfDocumentOnBrowser)}) with the following details : - ");
            }
            return File("", "application/pdf");
        }
        [HttpGet("assessmentsLetter")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGeneratedAssessmentPdfDocumentOnBrowser()
        {
            var resultTemplate = string.Empty;
            var assRefNo = string.Empty;
            var agencyCode = string.Empty;
            var confirmBarcode = string.Empty;
            var howToPayBarcode = string.Empty;
            byte[] file;
            try
            {
                //https://localhost:44342/api/Payvalue/assessmentLetter?&T0dTUz4zN=UFZPRzIxOVRUMDQ1LzQwMDA3MDAyMTExMDAwMA
                //https://localhost:44342/api/Payvalue/assessmentLetter?T0dTUz4zN=UFZPRzIxMEFBMDMx
                var encodedString = AppWebExtension.Base64UrlEncode("PVOG231LL49F/400070021110000");
                string sentString = HttpContext.Request.Query["T0dTUz4zN"];
                if (sentString.Length < 0)
                {
                    resultTemplate = _entityManager.EmptyPdf();
                }
                else
                {
                    var decodedString = AppWebExtension.Base64UrlDecode(sentString.ObjectToString());
                    var stringSplit = decodedString.Split("/");
                    if (stringSplit.Length != 2)
                    {
                        var msg = "Invalid Information";
                        resultTemplate = _entityManager.EmptyPdf();
                    }

                    assRefNo = stringSplit[0];
                    agencyCode = stringSplit[1];
                    //var formulatedString = assRefNo + "/" + agencyCode;
                    //var encodeAssessmentRefNo = AppWebExtension.Base64UrlEncode(formulatedString.TrimEnd().ObjectToString());
                    //_logger.LogInformation("This is the encoded assessment ref no: {@Request}", sentString);
                    //Log.Information("This is the encoded assessment ref no [S]: {@Request}", sentString);
                    //Log.Information("Finished Seeding Default Data09888800880");


                    confirmBarcode = $"{_appSettings.ServiceBaseUrl}/api/Payvalue/assessmentsLetter?&T0dTUz4zN={sentString}";
                    howToPayBarcode = $"{_appSettings.ServiceBaseUrl}/api/Payvalue/paymentInstructions";

                    //_logger.LogInformation("This is the confirm barcode that will be encoded: {@Request}", confirmBarcode);
                    //_logger.LogInformation("This is the how to pay that will be encoded: {@Request}", howToPayBarcode);

                    //Log.Information("This is the confirm barcode that will be encoded [S]: {@Request}", confirmBarcode);
                    //Log.Information("This is the how to pay that will be encoded [S]: {@Request}", howToPayBarcode);

                }


                var sentDocumentTitle = "PDF Report"; // sentdocumentTitleHeadersDesirialized;
                var sentPowerBy = "Powered By" + " " + " " + "http://icmaservices.com"; //sentPowerByHeaders
                var sentUrl = "https://payment.deltabir.com/";


                var assessmentDetail = new List<AssessmentDTO>();
                var additionalInfo = new List<BillAdditionalInfoDto>();
                assessmentDetail = await _entityManager.GetAllApprovedAssessmentNotice(agencyCode, assRefNo).ConfigureAwait(false);
                if (assessmentDetail.Any())
                {
                    var barCodeRequest = new BarCodeRequest
                    {
                        rawString = confirmBarcode
                    };

                    var confirmDocUrl = await _entityManager.GenerateConfirmDocBarcode(confirmBarcode);
                    var howToPayUrl = await _entityManager.GenerateBarcode(howToPayBarcode);

                    //_logger.LogInformation("This is the confirm doc barcode response: {@Request}", confirmBarcode);
                    //_logger.LogInformation("This is the how to pay url response: {@Request}", howToPayBarcode);

                    Log.Information("This is the confirm doc barcode response [S]: {@Request}", confirmDocUrl);
                    Log.Information("This is the how to pay url response [S]: {@Request}", howToPayUrl);

                    confirmDocUrl = string.IsNullOrEmpty(confirmDocUrl) || string.IsNullOrWhiteSpace(confirmDocUrl) ? howToPayUrl : confirmDocUrl;

                    var billInfoGuid = assessmentDetail.FirstOrDefault().BillInfoGuid;
                    additionalInfo = await _entityManager.GetAllAdditionalInfo(billInfoGuid);

                    var assessmentDetailFirstDefault = assessmentDetail.FirstOrDefault();
                    if (assessmentDetailFirstDefault != null && assessmentDetailFirstDefault.ServiceId == _appSettings.ConsumptionTax)
                    {
                        resultTemplate = _entityManager.GetConsumptionTaxAssessmentNoticeHtml(assessmentDetail, additionalInfo, howToPayUrl, confirmDocUrl);
                    }
                    else
                    {
                        resultTemplate = _entityManager.GetAssessmentNoticeHTMLString(assessmentDetail, additionalInfo, howToPayUrl, confirmDocUrl);
                    }

                }
                else
                {
                    resultTemplate = _entityManager.AssessmentEmptyPdf();
                }

                //_logger.LogInformation("Result Template Response: {@Request}", resultTemplate);

                var pdfConverterRequest = new Application.DTOs.PdfConverterRequest()
                {
                    HtmlContent = resultTemplate,
                    //OutputFileName = sentDocumentTitle,
                    OutputFileName = assRefNo == null ? Guid.NewGuid().ToString() : assRefNo,
                };

                file = await _entityManager.ToPdf(pdfConverterRequest);
                //var pdfFilePath = await _repo.ConvertWithPlayWrightPdfUtilityAsync(pdfConverterRequest);

                ////Set the Content-Disposition header so as to display on the browser
                Response.Headers.Append("Content-Disposition", $"inline; filename={sentDocumentTitle}.pdf");

                return File(file, "application/pdf");
                //Showing a PDF Document in a Browser  //Note to use both Enabling Download Mode and Showing a PDF Document in a Browser, you must comment Out in globalSettings above.

                //if (pdfFilePath == null)
                //{
                //    return BadRequest("Failed to generate the PDF.");
                //}

                //// Serve the file in the response to download or display in the browser
                //var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfFilePath);
                //var fileName = Path.GetFileName(pdfFilePath);

                //Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");

                //return File(fileBytes, "application/pdf", fileName);

            }
            catch (Exception ex)
            {
                Log.Error($"An Exception Error as occurred in an Application Url ({nameof(OpenGeneratedAssessmentPdfDocumentOnBrowser)}) with the following details : - ");
            }
            return File("", "application/pdf");
        }

    }
}

