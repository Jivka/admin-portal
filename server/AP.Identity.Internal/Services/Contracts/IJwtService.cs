using AP.Common.Data.Identity.Entities;
using AP.Common.Models;

namespace AP.Identity.Internal.Services.Contracts;

public interface IJwtService
{
    string GenerateJwtToken(User user, List<string> roles);
    string GenerateJwtToken(User user, List<TenantRole> tenantRoles);
}