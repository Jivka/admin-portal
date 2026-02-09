using System.Reflection;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AP.Common.Data.Options;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Extensions;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services;
using AP.Identity.Internal.Services.Contracts;
using AP.Identity.Internal.Services.Processors;

namespace AP.Identity.Internal;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentitySettings>(configuration.GetSection(nameof(IdentitySettings)));

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<ITenantsService, TenantsService>();
        services.AddScoped<ITenantUsersService, TenantUsersService>();
        services.AddScoped<ISystemService, SystemService>();

        services.AddSingleton(Channel.CreateUnbounded<UserEventRequest>());
        services.AddSingleton(Channel.CreateUnbounded<SendEmailRequest>());
        // background services to process the channels
        services.AddHostedService<UserEventProcessor>();
        services.AddHostedService<SendEmailProcessor>();

        services.AddAutoMapperProfile(Assembly.GetExecutingAssembly());

        return services;
    }
}