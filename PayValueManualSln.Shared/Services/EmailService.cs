using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PayValueManualSln.Application.DTOs.Email;
using PayValueManualSln.Application.Exceptions;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Entities.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PayValueManualSln.Shared.Services
{
    public class EmailService : IEmailService
    {
        public MailSettings _mailSettings { get; }
        public MailSettingsCredentials _mailMessageSettings { get; }
        public ILogger<EmailService> _logger { get; }

        public EmailService(IOptions<MailSettings> mailSettings, IOptions<MailSettingsCredentials> mailMessageSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _mailMessageSettings = mailMessageSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequest request)
        {
            try
            {
                // create message
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(request.From ?? _mailSettings.EmailFrom);
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = request.Body;
                email.Body = builder.ToMessageBody();
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                
            }
        }
        public async Task SendAsync(string to, string subject, string html, string from = null, List<IFormFile> files = null)
        {
            try
            {
                // create message
                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(from ?? _mailSettings.EmailFrom)
                };
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                var builder = new BodyBuilder();

                if (files != null)
                {
                    byte[] fileBytes;
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }

                            builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }
                builder.HtmlBody = html;
                email.Body = builder.ToMessageBody();
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message, ex);
              
            }
        }


        public void SendEmail(string toEmail, string toName, string Subject, string Message)
        {
            using (var message = new MailMessage())
            {
                string from = _mailMessageSettings.EmailFrom;

                message.To.Add(new MailAddress(toEmail, toName));
                message.From = new MailAddress(from, _mailMessageSettings.DisplayName);
                //message.CC.Add(new MailAddress("cc@email.com", "CC Name"));
                //message.Bcc.Add(new MailAddress("bcc@email.com", "BCC Name"));
                message.Subject = Subject;
                message.Body = Message;
                message.IsBodyHtml = true;

                using (var client = new System.Net.Mail.SmtpClient(_mailMessageSettings.SmtpHost))
                {
                    client.Port = _mailMessageSettings.SmtpPort;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_mailMessageSettings.SmtpUser, _mailMessageSettings.SmtpPass);
                    client.EnableSsl = true;
                    ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                        //do nothing
                    }
                }
            }
        }
        public void SendBulkEmail(List<string> toAddress, string fromEmail, string fromName, string Subject, string Message)
        {
            using (var message = new MailMessage())
            {
                string from = _mailMessageSettings.EmailFrom;

                toAddress.ForEach(address => message.To.Add(new MailAddress(address)));
                message.From = new MailAddress(from, fromName);
                //message.CC.Add(new MailAddress("cc@email.com", "CC Name"));
                //message.Bcc.Add(new MailAddress("bcc@email.com", "BCC Name"));
                message.Subject = Subject;
                message.Body = Message;
                message.IsBodyHtml = true;


                using (var client = new System.Net.Mail.SmtpClient(_mailSettings.SmtpHost))
                {
                    client.Port = _mailSettings.SmtpPort;
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                    client.EnableSsl = true;
                    ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                    client.Send(message);
                }
            }
        }
        public void SendEmailWithAttachment(string toEmail, string toName, string fromName, string Subject, string Message, string attachmentFileName)
        {
            using (var message = new MailMessage())
            {
                string from = _mailMessageSettings.EmailFrom;

                message.To.Add(new MailAddress(toEmail, toName));
                message.From = new MailAddress(from, fromName);
                Attachment attachment;
                attachment = new Attachment(attachmentFileName);
                message.Attachments.Add(attachment);                //message.CC.Add(new MailAddress("cc@email.com", "CC Name"));
                //message.Bcc.Add(new MailAddress("bcc@email.com", "BCC Name"));
                message.Subject = Subject;
                message.Body = Message;
                message.IsBodyHtml = true;


                using (var client = new System.Net.Mail.SmtpClient(_mailSettings.SmtpHost))
                {
                    client.Port = _mailSettings.SmtpPort;
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                    client.EnableSsl = true;
                    ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                    client.Send(message);
                }
            }
        }
    }
}
