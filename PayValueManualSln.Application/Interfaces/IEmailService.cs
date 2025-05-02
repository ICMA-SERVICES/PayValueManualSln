using Microsoft.AspNetCore.Http;
using PayValueManualSln.Application.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequest request);
        Task SendAsync(string to, string subject, string html, string from = null, List<IFormFile> files = null);

        void SendEmailWithAttachment(string toEmail, string toName, string fromName, string Subject, string Message, string attachmentFileName);
        void SendBulkEmail(List<string> toAddress, string fromEmail, string fromName, string Subject, string Message);
        void SendEmail(string toEmail, string toName, string Subject, string Message);
    }
}
