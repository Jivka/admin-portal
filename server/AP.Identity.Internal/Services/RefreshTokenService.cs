using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using AP.Common.Data;
using AP.Common.Data.Identity.Entities;
using AP.Common.Data.Options;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.NoticeMessages;

namespace AP.Identity.Internal.Services;

public class RefreshTokenService(IOptions<IdentitySettings> identitySettings, DataContext dbContext) : IRefreshTokenService
{
    private readonly IdentitySettings identitySettings = identitySettings.Value;
    ////private readonly DataContext dbContext = dbContext;

    public RefreshToken GenerateRefreshToken(string? ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            // refresh token as cryptographically strong random sequence of values
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            // refresh token to be valid for 7 days
            ExpiresOn = DateTime.UtcNow.AddDays(identitySettings.RefreshTokenTTE),
            CreatedOn = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
        //ensure token is unique by checking against db
        var tokenIsUnique = !dbContext.Users
            .SelectMany(user => user.RefreshTokens!)
            .Any(token => token != null && token.Token == refreshToken.Token);
        if (!tokenIsUnique)
        {
            refreshToken = GenerateRefreshToken(ipAddress);
        }

        return refreshToken;
    }

    public void RemoveOldRefreshTokens(User user)
    {
        user.RefreshTokens?.RemoveAll(x =>
            !x.IsActive &&
            x.CreatedOn.AddDays(identitySettings.RefreshTokenTTL) <= DateTime.UtcNow);
    }

    public void RevokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string? ipAddress, string reason)
    {
        // recursively traverse the refresh token chain and ensure all descendants are revoked
        if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            var childToken = user.RefreshTokens?.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
            if (childToken == null)
            {
                return;
            }
            if (childToken.IsActive)
            {
                RevokeRefreshToken(childToken, ipAddress, reason, null);
            }
            else
            {
                RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }
    }

    public RefreshToken RotateRefreshToken(RefreshToken refreshToken, string? ipAddress)
    {
        var newRefreshToken = GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, RevokedWithReplacement, newRefreshToken.Token);
        return newRefreshToken;
    }

    public void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason, string? replacedByToken)
    {
        token.RevokedOn = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
}