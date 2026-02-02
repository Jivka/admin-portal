using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AP.Common.Data;

public partial class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);

        CreateIdentity(modelBuilder);
    }
}