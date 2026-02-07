using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AP.Common.Data.Contracts;
using AP.Common.Data.Extensions;
using AP.Common.Data.Identity.Entities;
using AP.Common.Data.Identity.Enums;
using AP.Common.Data.Options;

namespace AP.Common.Data.Identity;

public class DataSeeder(DataContext dbContext, IOptions<IdentitySettings> identitySettings) : IDataSeeder
{
    private readonly IdentitySettings identitySettings = identitySettings.Value;

    public void SeedData()
    {
        Task
            .Run(async () =>
            {
                SeedRoles();
                SeedTenantTypes();
                SeedTenantOwnerships();

                var systemAdminUserTenantId = SeedAdminUser();
                identitySettings.SystemTenantId = systemAdminUserTenantId ?? default;

                if (dbContext.ChangeTracker.HasChanges())
                {
                    await dbContext.SaveChangesAsync();
                }
            })
        .GetAwaiter()
        .GetResult();
    }

    private void SeedRoles()
    {
        if (!dbContext.Roles.Any())
        {
            var roles = GetRoles();
            dbContext.Roles.AddRange(roles);
        }
    }

    // seed tenant types
    private void SeedTenantTypes()
    {
        if (!dbContext.TenantTypes.Any())
        {
            var tenantTypes = GetTenantTypes();
            dbContext.TenantTypes.AddRange(tenantTypes);
        }
    }

    // seed tenant ownerships
    private void SeedTenantOwnerships()
    {
        if (!dbContext.TenantOwnerships.Any())
        {
            var tenantOwnerships = GetTenantOwnerships();
            dbContext.TenantOwnerships.AddRange(tenantOwnerships);
        }
    }

    private int? SeedAdminUser()
    {
        int? systemAdminUserTenantId;

        // seed db with 1 Admin user  
        if (!dbContext.Users.Any() || !dbContext.UserTenants.Any(ut => ut.RoleId == (byte)Roles.SystemAdmin))
        {
            var adminUser = GetAdminUser();
            dbContext.Users.Add(adminUser);

            systemAdminUserTenantId = adminUser?.UserTenants?.FirstOrDefault()?.TenantId;

            // send verify-email email message
        }
        else
        {
            systemAdminUserTenantId = dbContext.UserTenants
                .AsNoTracking()
                .FirstOrDefault(ut => ut.RoleId == (byte)Roles.SystemAdmin)?
                .TenantId;
        }

        return systemAdminUserTenantId;
    }

    private static List<Role> GetRoles()
    {
        List<Role> roles = [];
        foreach (var r in Enum.GetValues<Roles>())
        {
            roles.Add(CreateRoleFromEnum(r));
        }

        return roles;
    }

    private static Role CreateRoleFromEnum(Roles role)
    {
        return new Role()
        {
            RoleId = (byte)role,
            RoleName = role.ToString(),
            RoleDisplayName = role.GetDisplayName(),
            RoleSummary = role.GetDescription()
        };
    }

    private static List<TenantType> GetTenantTypes()
    {
        List<TenantType> tenantTypes = [];
        foreach (var t in Enum.GetValues<TenantTypes>())
        {
            tenantTypes.Add(CreateTenantTypeFromEnum(t));
        }
        return tenantTypes;
    }

    private static TenantType CreateTenantTypeFromEnum(TenantTypes tenantType)
    {
        return new TenantType()
        {
            TypeId = (byte)tenantType,
            TypeName = tenantType.ToString(),
            TypeSummary = tenantType.GetDisplayName()
        };
    }

    // create tenant ownerships from enum
    private static List<TenantOwnership> GetTenantOwnerships()
    {
        List<TenantOwnership> tenantOwnerships = [];
        foreach (var o in Enum.GetValues<TenantOwnerships>())
        {
            tenantOwnerships.Add(CreateTenantOwnershipFromEnum(o));
        }
        return tenantOwnerships;
    }

    private static TenantOwnership CreateTenantOwnershipFromEnum(TenantOwnerships ownership)
    {
        return new TenantOwnership()
        {
            OwnershipId = (byte)ownership,
            OwnershipName = ownership.ToString(),
            OwnershipSummary = ownership.GetDisplayName()
        };
    }

    private User GetAdminUser()
    {
        return new User
        {
            FirstName = "Admin",
            Email = $"admin{identitySettings.EmailDomain}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(identitySettings.InitialPassword),
            Active = true,
            Enabled = true,
            CreatedOn = DateTime.UtcNow,
            VerifiedOn = DateTime.UtcNow,
            UserTenants =
            [
                new UserTenant
                {
                    Tenant = new Tenant
                    {
                        TenantName = "Admin Tenant",
                        TenantBIC = "ADMINBIC",
                        TenantType = (byte)TenantTypes.Company,
                        Ownership = (byte)TenantOwnerships.Owner,
                        Active = true,
                        Enabled = true,
                        CreatedOn = DateTime.UtcNow
                    },
                    RoleId = (byte)Roles.SystemAdmin,
                    CreatedOn = DateTime.UtcNow
                }
            ]
        };
    }
}