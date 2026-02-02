using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using AP.Common.Services.Contracts;
using AP.Common.Data.Options;

namespace AP.Common.Services;

public class EmailService(IOptions<EmailSettings> emailSettings, IWebHostEnvironment hostEnvironment) : IEmailService
{
    private readonly EmailSettings emailSettings = emailSettings.Value;
    private readonly IWebHostEnvironment hostEnvironment = hostEnvironment;

    public async Task Send(string to, string subject, string html, string? from = null)
    {
        // include logo image
        var builder = new BodyBuilder();
        var imagePath = Path.Combine(hostEnvironment.WebRootPath, "images", "logo.png");
        var image = await builder.LinkedResources.AddAsync(imagePath);
        image.ContentId = MimeUtils.GenerateMessageId();
        builder.HtmlBody = string.Format(@"<img src=""cid:{0}"" width=""20%"" height=""20%"">" + html, image.ContentId);

        // create message
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(from ?? emailSettings.EmailFrom));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = builder.ToMessageBody();

        // send email
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(emailSettings.SmtpHost, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(emailSettings.SmtpUser, emailSettings.SmtpPass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
