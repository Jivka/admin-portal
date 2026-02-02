using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using AP.Common.Services.Contracts;
using AP.Identity.Internal.Models;

namespace AP.Identity.Internal.Services.Processors;

public class SendEmailProcessor(Channel<SendEmailRequest> channel, IEmailService emailService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await emailService.Send(
                to: request.To,
                subject: request.Subject,
                html: request.Html
            );
        }
    }
}