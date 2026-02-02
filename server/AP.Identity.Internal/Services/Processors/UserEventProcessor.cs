using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using AP.Common.Data.Identity.Entities;
using AP.Identity.Internal.Models;

namespace AP.Identity.Internal.Services.Processors;

public class UserEventProcessor(Channel<UserEventRequest> channel, DbContext dbContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await dbContext.Set<UserEvent>().AddAsync(new UserEvent()
            {
                EventId = request.EventId,
                UserId = request.UserId,
                UserSub = request.UserSub,
                Username = request.Username,
                UserEmail = request.UserEmail,
                Action = request.Action,
                Success = request.Success,
                ActionOn = request.ActionOn,
                ActionFromIp = request.ActionFromIp
            }, stoppingToken);

            // save event in db
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}