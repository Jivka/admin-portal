using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AP.Common.Data.Contracts;

namespace AP.Common.Utilities.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder Initialize(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var serviceProvider = serviceScope.ServiceProvider;
        var db = serviceProvider.GetRequiredService<DbContext>();

        if (env.EnvironmentName == "Local")
        {
            _ = db.Database.EnsureCreated();
        }

        if (env.IsDevelopment())
        {
            db.Database.Migrate();
        }

        var seeders = serviceProvider.GetServices<IDataSeeder>();
        foreach (var seeder in seeders)
        {
            seeder.SeedData();
        }

        return app;
    }
}