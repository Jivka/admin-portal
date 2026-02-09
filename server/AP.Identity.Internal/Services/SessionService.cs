using Microsoft.EntityFrameworkCore;
using AP.Common.Data;
using AP.Common.Data.Identity.Entities;
using AP.Common.Services.Contracts;

namespace AP.Identity.Internal.Services;

public class SessionService(DataContext dbContext) : ISessionService
{
    public async Task<UserSession> CreateSession(int userId, string accessToken, string refreshToken, string? ipAddress)
    {
        // Check if a session already exists for this user and IP
        var existingSession = await dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CreatedFomIp == ipAddress);

        if (existingSession != null)
        {
            // Update existing session
            existingSession.AccessToken = accessToken;
            existingSession.RefreshToken = refreshToken;
            existingSession.CreatedOn = DateTime.UtcNow;
            
            await dbContext.SaveChangesAsync();
            return existingSession;
        }

        // Create new session
        var session = new UserSession
        {
            UserId = userId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            CreatedOn = DateTime.UtcNow,
            CreatedFomIp = ipAddress
        };

        dbContext.UserSessions.Add(session);
        await dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<UserSession?> GetSessionById(long sessionId)
    {
        return await dbContext.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<UserSession?> GetSessionByUserAndIp(int userId, string? ipAddress)
    {
        return await dbContext.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CreatedFomIp == ipAddress);
    }

    public async Task UpdateSession(UserSession session, string newAccessToken, string newRefreshToken)
    {
        session.AccessToken = newAccessToken;
        session.RefreshToken = newRefreshToken;
        session.CreatedOn = DateTime.UtcNow;

        dbContext.UserSessions.Update(session);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteSession(long sessionId)
    {
        var session = await dbContext.UserSessions.FindAsync(sessionId);
        if (session != null)
        {
            dbContext.UserSessions.Remove(session);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteAllUserSessions(int userId)
    {
        var sessions = await dbContext.UserSessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        dbContext.UserSessions.RemoveRange(sessions);
        await dbContext.SaveChangesAsync();
    }

    public async Task CleanupExpiredSessions()
    {
        // Remove sessions older than 30 days
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var expiredSessions = await dbContext.UserSessions
            .Where(s => s.CreatedOn < cutoffDate)
            .ToListAsync();

        dbContext.UserSessions.RemoveRange(expiredSessions);
        await dbContext.SaveChangesAsync();
    }
}
