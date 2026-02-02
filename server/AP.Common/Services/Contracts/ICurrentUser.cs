namespace AP.Common.Services.Contracts;

public interface ICurrentUser
{
    int UserId { get; }
    string? UserSub { get; }
    string? Username { get; }
    string? Nickname { get; }
    string? GivenName { get; }
    string? FamilyName { get; }
    string? Email { get; }
    bool? EmailVerified { get; }
    string? Picture { get; }

    string? TenantRoles { get; }

    bool IsSystemAdministrator { get; }

    bool IsTestUser { get; }

    string? IpAddress { get; }
    string? TraceId { get; init; }
    DateTime RequestCreatedOn { get; }
}