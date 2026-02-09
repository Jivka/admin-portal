using AP.Common.Data.Identity.Entities;

namespace AP.Common.Services.Contracts;

public interface ISessionService
{
    Task<UserSession> CreateSession(int userId, string accessToken, string refreshToken, string? ipAddress);
    Task<UserSession?> GetSessionById(long sessionId);
    Task<UserSession?> GetSessionByUserAndIp(int userId, string? ipAddress);
    Task UpdateSession(UserSession session, string newAccessToken, string newRefreshToken);
    Task DeleteSession(long sessionId);
    Task DeleteAllUserSessions(int userId);
    Task CleanupExpiredSessions();
}
