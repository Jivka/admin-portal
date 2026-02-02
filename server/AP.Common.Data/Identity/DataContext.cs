using Microsoft.EntityFrameworkCore;
using AP.Common.Data.Identity.Entities;

namespace AP.Common.Data;

public partial class DataContext : DbContext
{
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<UserEvent> UserEvents { get; set; } = default!;
    public DbSet<UserSession> UserSessions { get; set; } = default!;

    public DbSet<Tenant> Tenants { get; set; } = default!;
    public DbSet<Contact> Contacts { get; set; } = default!;
    public DbSet<TenantType> TenantTypes { get; set; } = default!;
    public DbSet<TenantOwnership> TenantOwnerships { get; set; } = default!;

    public DbSet<UserTenant> UserTenants { get; set; } = default!;

    protected internal static void CreateIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.Property(c => c.Active)
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValueSql("1");
            b.Property(c => c.Enabled)
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValueSql("1");

            b.HasIndex(a => a.Email)
                .IsUnique()
                .HasDatabaseName("IX_Email");
        });

        modelBuilder.Entity<UserSession>(b =>
        {
            b.HasIndex(a => new { a.UserId, a.CreatedFomIp })
                .IsUnique()
                .HasDatabaseName("IX_UserSessions");

            b.HasOne(c => c.User).WithMany(s => s.Sessions).OnDelete(DeleteBehavior.ClientCascade);
        });

        modelBuilder.Entity<Tenant>(b =>
        {
            b.Property(c => c.Active)
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValueSql("1");
            b.Property(c => c.Enabled)
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValueSql("1");

            b.HasIndex(a => a.TenantName)
                .IsUnique()
                .HasDatabaseName("IX_TenantName");
            b.HasIndex(a => a.TenantBIC)
                .IsUnique()
                .HasDatabaseName("IX_TenantBIC");
            b.HasOne(c => c.TenantTypeObj).WithMany().OnDelete(DeleteBehavior.ClientCascade);
            b.HasOne(c => c.TenantOwnership).WithMany().OnDelete(DeleteBehavior.ClientCascade);
            b.HasOne(c => c.CreatedByUser).WithMany().OnDelete(DeleteBehavior.ClientCascade);
        });

        modelBuilder.Entity<TenantContact>(b =>
        {
            b.HasKey(c => new { c.TenantId, c.ContactId });

            b.HasOne(c => c.Contact).WithMany().OnDelete(DeleteBehavior.ClientCascade);
        });

        modelBuilder.Entity<UserTenant>(b =>
        {
            b.HasKey(c => new { c.UserId, c.TenantId });

            b.HasOne(c => c.User).WithMany(s => s.UserTenants).OnDelete(DeleteBehavior.ClientCascade);
            b.HasOne(c => c.Tenant).WithMany().OnDelete(DeleteBehavior.ClientCascade);
            b.HasOne(c => c.Role).WithMany().OnDelete(DeleteBehavior.ClientCascade);
            b.HasOne(c => c.CreatedByUser).WithMany().OnDelete(DeleteBehavior.ClientCascade);
        });
    }
}