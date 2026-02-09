using System.Reflection;
using System.Text;
using Swashbuckle.AspNetCore.Filters;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                }
            };
        });

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

        // Don't set default schemes here - let the calling code decide
        // This prevents conflicts when multiple authentication schemes are registered
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

                bearer.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // SessionAuthenticationMiddleware adds the token to Authorization header
                        // This event is called before the default token extraction happens
                        // We don't need to do anything here - the default handler will read from the header
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures for debugging
                        var logger = context.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<JwtBearerEvents>>();
                        logger?.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Suppress redirect for API requests
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsJsonAsync(new
                            {
                                error = "Unauthorized",
                                message = "Authentication required. Please login to obtain a session."
                            });
                        }
                        return Task.CompletedTask;
                    }
                };

                if (events != null)
                {
                    // Merge custom events if provided
                    var onMessageReceived = bearer.Events.OnMessageReceived;
                    var onChallenge = bearer.Events.OnChallenge;
                    
                    bearer.Events.OnMessageReceived = async context =>
                    {
                        await onMessageReceived(context);
                        if (events.OnMessageReceived != null)
                        {
                            await events.OnMessageReceived(context);
                        }
                    };
                    
                    if (events.OnChallenge != null)
                    {
                        bearer.Events.OnChallenge = async context =>
                        {
                            await onChallenge(context);
                            await events.OnChallenge(context);
                        };
                    }
                }
            });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentToken, CurrentToken>();
        services.AddScoped<JwtHeaderAuthenticationMiddleware, JwtHeaderAuthenticationMiddleware>();
        services.AddScoped<JwtCookieAuthenticationMiddleware, JwtCookieAuthenticationMiddleware>();
        services.AddScoped<SessionAuthenticationMiddleware, SessionAuthenticationMiddleware>();

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
                Description = "Session-based authentication using SessionId cookie. The JWT token is retrieved from the server-side session automatically. Login using the Swagger UI login form to authenticate.",
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