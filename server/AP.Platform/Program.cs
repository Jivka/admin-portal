using System.Reflection;
using System.Text.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using AP.Common.Data;
using AP.Common.Data.Options;
using AP.Common.Services;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Converters;
using AP.Common.Utilities.Extensions;
using AP.Common.Utilities.Handlers;
using AP.Common.Utilities.Middleware;
using AP.Identity.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDatabase<DataContext>(builder.Configuration);

builder.Services.AddIdentityModule(builder.Configuration);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));

// Add global services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<SwaggerAuthenticationMiddleware, SwaggerAuthenticationMiddleware>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAutoMapperProfile(Assembly.GetExecutingAssembly());

builder.Services.AddTokenAuthentication(builder.Configuration);
builder.Services.AddCookieAuthentication();

builder.Services.AddCors();
builder.Services.AddRazorPages();
builder.Services.AddAuthorizationCore();
builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.DefaultBufferSize = 51200;

        opts.JsonSerializerOptions.Converters.Add(new StrictStringEnumConverterFactory());
        opts.JsonSerializerOptions.Converters.Add(new DecimalToStringWriterConverter());
    });
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSwaggerServices();
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Host.UseSerilog((hostBuilder, serviceProvider, config) =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

var app = builder.Build();

// global error handler
app.UseExceptionHandler("/Home/Error");

app.Initialize(builder.Environment);

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<JwtCookieAuthenticationMiddleware>();
app.UseAuthentication();

// Swagger OAuth
app.UseMiddleware<SwaggerAuthenticationMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    ////foreach (var api in SwaggerConfig.Config)
    ////{
    ////    options.SwaggerEndpoint(api.GetSwaggerEndpoint(), api.Version);
    ////}

    options.InjectStylesheet("/swagger/swagger.css");
    options.InjectJavascript("/swagger/swagger.js");
    ////options.OAuthUsePkce();
    options.DefaultModelsExpandDepth(-1);
});

app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.UseCors(options => options
.SetIsOriginAllowed(_ => true)
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials());

app.MapRazorPages();
app.MapDefaultControllerRoute();

await app.RunAsync();