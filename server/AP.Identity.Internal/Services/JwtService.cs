using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AP.Common.Data.Identity.Entities;
using AP.Common.Data.Options;
using AP.Common.Models;
using AP.Identity.Internal.Services.Contracts;

namespace AP.Identity.Internal.Services;

public class JwtService(IOptions<IdentitySettings> identitySettings) : IJwtService
{
    private readonly IdentitySettings identitySettings = identitySettings.Value;

    public string GenerateJwtToken(User user, List<string> roles)
    {
        //OLD
        if (string.IsNullOrWhiteSpace(identitySettings.Secret))
        {
            return string.Empty;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(identitySettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.UserSub ?? user.FullName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            }),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(identitySettings.JwtTokenTTE),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        if (roles.Count != 0)
        {
            tokenDescriptor.Subject.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var encryptedToken = tokenHandler.WriteToken(token);

        return encryptedToken;
    }

    public string GenerateJwtToken(User user, List<TenantRole> tenantRoles)
    {
        // NEW
        if (string.IsNullOrWhiteSpace(identitySettings.Secret))
        {
            return string.Empty;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(identitySettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.UserSub ?? user.FullName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            }),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(identitySettings.JwtTokenTTE),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        if (tenantRoles.Count != 0)
        {
            tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, JsonSerializer.Serialize(tenantRoles)));
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var encryptedToken = tokenHandler.WriteToken(token);

        return encryptedToken;
    }
}