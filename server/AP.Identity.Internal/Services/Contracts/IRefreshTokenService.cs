using System.Collections.Generic;
using AP.Common.Data.Identity.Entities;

namespace AP.Identity.Internal.Services.Contracts;

public interface IRefreshTokenService
{
    RefreshToken GenerateRefreshToken(string? ipAddress);

    void RemoveOldRefreshTokens(User user);

    void RevokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string? ipAddress, string reason);

    void RevokeRefreshToken(RefreshToken token, string? ipAddress, string? reason, string? replacedByToken);

    RefreshToken RotateRefreshToken(RefreshToken refreshToken, string? ipAddress);
}