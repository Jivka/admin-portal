using System.Reflection;
using System.Text;
using Swashbuckle.AspNetCore.Filters;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AP.Common.Data.Contracts;
using AP.Common.Data.Identity;
using AP.Common.Data.Options;
using AP.Common.Services;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Middleware;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCookieAuthentication(
        this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        return services;
    }

    public static IServiceCollection AddTokenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        JwtBearerEvents? events = null)
    {
        var secret = configuration
            .GetSection(nameof(IdentitySettings))
            .GetValue<string>(nameof(IdentitySettings.Secret));

        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentNullException(nameof(configuration), "Secret cannot be null or empty.");
        }

        var key = Encoding.ASCII.GetBytes(secret);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                    {
                        return notBefore <= DateTime.UtcNow &&
                               expires >= DateTime.UtcNow;
                    },
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };

                if (events != null)
                {
                    bearer.Events = events;
                }
            });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentToken, CurrentToken>();
        services.AddScoped<JwtHeaderAuthenticationMiddleware, JwtHeaderAuthenticationMiddleware>();

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Description = "Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = BearerScheme
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = BearerScheme
                        },
                        Scheme = "oauth2",
                        Name = BearerScheme,
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });

            //Include all xml files
            Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList().ForEach(xmlFile => options.IncludeXmlComments(xmlFile, true));

            options.ExampleFilters();
            options.CustomSchemaIds(x => x.FullName);
        });
 
        return services;
    }

    public static IServiceCollection AddDatabase<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext
        => services
            .AddScoped<DbContext, TDbContext>()
            .AddDbContext<TDbContext>(options => options
                .UseSqlServer(configuration.GetConnectionString("APConnection"),
                    sqlOptions => sqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null)))
            .AddTransient<IDataSeeder, DataSeeder>();

    public static IServiceCollection AddAutoMapperProfile(
        this IServiceCollection services,
        Assembly assembly)
        => services
            .AddAutoMapper(
                (_, config) => config.AddProfile(new MappingProfile(assembly)),
                Array.Empty<Assembly>());

    public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}