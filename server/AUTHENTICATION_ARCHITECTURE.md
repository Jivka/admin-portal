# Session-Based Authentication Architecture

## Overview

The Admin Portal (AP) solution now uses a **session-based authentication architecture** where all controllers with authentication read the `SessionId` cookie and retrieve JWT access and refresh tokens from the server-side session.

## How It Works

### 1. **Login Flow**

When a user logs in via `/identity/sign-in` or `/auth/internal`:

1. **IdentityService** validates credentials and generates JWT tokens
2. A `UserSession` record is created in the database with:
   - `SessionId` (primary key)
   - `UserId`
   - `AccessToken` (JWT)
   - `RefreshToken`
   - `CreatedOn`
   - `CreatedFomIp`
3. Only the `SessionId` is sent to the client as an HTTP-only, Secure, SameSite=Strict cookie
4. The JWT tokens remain **server-side** and are never exposed to the client

### 2. **Authenticated Request Flow**

When a client makes a request to an authenticated endpoint:

1. **Client** sends request with `SessionId` cookie automatically included by the browser
2. **SessionAuthenticationMiddleware** intercepts the request:
   - Reads the `SessionId` cookie value
   - Queries the `UserSessions` table to retrieve the session
   - Extracts the `AccessToken` (JWT) from the session
   - Adds the JWT to the `Authorization` header: `Bearer {token}`
   - Stores the session in `HttpContext.Items["UserSession"]` for later use
3. **JWT Bearer Authentication** validates the token from the Authorization header
4. **Controller** receives the authenticated request with `User` principal populated

### 3. **Token Refresh Flow**

When tokens need to be refreshed via `/identity/refresh-token`:

1. Client sends request with `SessionId` cookie
2. **SessionAuthenticationMiddleware** retrieves the session and tokens
3. **IdentityController** calls `IdentityService.RefreshToken()`:
   - Validates the refresh token
   - Generates new access and refresh tokens
   - Updates the session in the database with new tokens
4. Client's `SessionId` cookie remains the same (session rotated, not recreated)

### 4. **Logout Flow**

When a user logs out via `/identity/logout`:

1. Client sends request with `SessionId` cookie
2. **IdentityController** deletes the session from the database
3. Server clears the `SessionId` cookie
4. Client is now logged out, session is destroyed

## Key Components

### SessionAuthenticationMiddleware

Located: `AP.Common\Utilities\Middleware\SessionAuthenticationMiddleware.cs`

**Responsibilities:**
- Read `SessionId` cookie from incoming requests
- Query `UserSessions` table to retrieve JWT tokens
- Add JWT `AccessToken` to `Authorization` header
- Store session in `HttpContext.Items` for later use
- Runs **before** JWT Bearer authentication middleware

**Code snippet:**
```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    var sessionIdCookie = context.Request.Cookies[SessionCookieName];
    
    if (!string.IsNullOrEmpty(sessionIdCookie) && long.TryParse(sessionIdCookie, out var sessionId))
    {
        var sessionService = context.RequestServices.GetService<ISessionService>();
        var session = await sessionService.GetSessionById(sessionId);
        
        if (session != null && !string.IsNullOrEmpty(session.AccessToken))
        {
            currentToken?.Set(session.AccessToken);
            context.Request.Headers.Append(AuthorizationHeaderName, $"{AuthorizationHeaderValuePrefix} {session.AccessToken}");
            context.Items["UserSession"] = session;
        }
    }

    await next.Invoke(context);
}
```

### SessionService

Located: `AP.Identity.Internal\Services\SessionService.cs`

**Responsibilities:**
- Create/update sessions in `UserSessions` table
- Retrieve sessions by `SessionId` or `UserId + IP`
- Delete sessions (logout)
- Clean up expired sessions

**Key Methods:**
- `CreateSession()` - Creates or updates a session
- `GetSessionById()` - Retrieves session by ID
- `UpdateSession()` - Updates tokens in existing session
- `DeleteSession()` - Removes session (logout)

### IdentityService

Located: `AP.Identity.Internal\Services\IdentityService.cs`

**Key Changes:**
- `SignIn()` - Creates session and sets `SessionId` cookie
- `RefreshToken()` - Updates existing session with new tokens
- Private method `SetSessionCookie()` - Sets HTTP-only session cookie

## Controllers with Authentication

All controllers that require authentication now automatically work with session-based auth:

### System Admin Controllers
- **UsersController** - `[AuthorizeAdministrator]`
  - `GET /api/users` - List users
  - `GET /api/users/{userId}` - Get user
  - `POST /api/users` - Create user
  - `PUT /api/users` - Edit user
  - `DELETE /api/users/{userId}` - Delete user
  
- **TenantsController** - `[AuthorizeAdministrator]`
  - `GET /api/tenants` - List tenants
  - `GET /api/tenants/{tenantId}` - Get tenant
  - `POST /api/tenants` - Create tenant
  - `PUT /api/tenants/{tenantId}` - Edit tenant
  - `DELETE /api/tenants/{tenantId}` - Delete tenant

### Tenant Admin Controllers
- **TenantUsersController** - `[AuthorizeTenantAdministrator]`
  - `GET /api/tenants/users/{tenantId}` - List tenant users
  - `POST /api/tenants/users/{tenantId}` - Create tenant user
  - `PUT /api/tenants/users/{tenantId}` - Edit tenant user
  - `DELETE /api/tenants/users/{tenantId}/{userId}` - Delete tenant user

- **TenantsProfileController** - `[AuthorizeTenantAdministrator]`
  - `GET /api/tenants/profile` - Get user's tenants
  - `GET /api/tenants/profile/{tenantId}` - Get tenant profile
  - `PUT /api/tenants/profile/{tenantId}` - Edit tenant profile

All these controllers automatically:
1. Read the `SessionId` cookie (via `SessionAuthenticationMiddleware`)
2. Retrieve JWT tokens from the server-side session
3. Validate the JWT token
4. Populate the `User` principal with claims
5. Enforce authorization attributes

## Database Schema

### UserSessions Table

```sql
CREATE TABLE [UserSessions] (
    [SessionId] bigint NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [AccessToken] nvarchar(4096) NOT NULL,
    [RefreshToken] nvarchar(4096) NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [CreatedFomIp] nvarchar(64) NULL,
    CONSTRAINT [PK_UserSessions] PRIMARY KEY ([SessionId]),
    CONSTRAINT [FK_UserSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);

CREATE UNIQUE INDEX [IX_UserSessions] ON [UserSessions] ([UserId], [CreatedFomIp]);
```

**Key Points:**
- Unique constraint on `(UserId, CreatedFomIp)` - one session per user per IP
- `AccessToken` and `RefreshToken` stored server-side (never sent to client)
- `CreatedOn` used for session expiration tracking

## Security Benefits

### 1. **Token Protection**
- JWT tokens never sent to client
- No risk of token theft via XSS attacks
- Tokens cannot be extracted from browser storage

### 2. **Session Control**
- Server can invalidate sessions immediately
- Logout is truly server-side (not just client-side cookie deletion)
- Admin can revoke user sessions remotely

### 3. **Cookie Security**
- Only `SessionId` cookie sent to client
- HTTP-only flag prevents JavaScript access
- Secure flag ensures HTTPS-only transmission
- SameSite=Strict prevents CSRF attacks

### 4. **Audit Trail**
- All sessions tracked in database
- IP address logged for each session
- Session creation and expiration timestamps

## Configuration

### appsettings.json

```json
{
  "IdentitySettings": {
    "Secret": "your-secret-key-min-32-chars",
    "JwtTokenTTE": 1,      // JWT access token expiration in days
    "RefreshTokenTTE": 7    // Refresh token and session expiration in days
  }
}
```

### Constants

```csharp
// AP.Common\Constants\Constants.cs
public const string SessionCookieName = "SessionId";
public const string AuthorizationHeaderName = "Authorization";
public const string AuthorizationHeaderValuePrefix = "Bearer";
```

## Middleware Pipeline (Program.cs)

```csharp
app.UseHttpsRedirection();
app.UseMiddleware<SessionAuthenticationMiddleware>();  // ? Session middleware
app.UseAuthentication();                               // ? JWT Bearer authentication
app.UseAuthorization();                                // ? Authorization
```

**Order is critical:**
1. `SessionAuthenticationMiddleware` extracts token from session
2. `UseAuthentication()` validates the JWT token
3. `UseAuthorization()` enforces authorization policies

## Migration from Cookie-Based to Session-Based

### What Changed

**Before (Cookie-Based):**
- JWT tokens stored in cookies (`AuthenticationCookieName`, `RefreshTokenCookieName`)
- `JwtCookieAuthenticationMiddleware` read tokens from cookies
- Tokens exposed to client (albeit in HTTP-only cookies)

**After (Session-Based):**
- Only `SessionId` stored in cookie
- `SessionAuthenticationMiddleware` retrieves tokens from database
- JWT tokens never leave the server
- Controllers remain unchanged (transparent migration)

### Deprecated Components

- **JwtCookieAuthenticationMiddleware** - Marked as `[Obsolete]`, use `SessionAuthenticationMiddleware`
- **AuthenticationCookieName** constant - No longer used
- **RefreshTokenCookieName** constant - No longer used

### Breaking Changes

**None for controllers** - All authenticated controllers automatically work with the new session-based approach. The change is transparent to controller code.

## Testing

### Unit Tests

Tests in `AP.Identity.Internal.Tests` have been updated to work with session-based authentication:

- **UsersControllerTests** - Mock session service and session cookie
- **IdentityControllerTests** - Test session creation, refresh, and deletion

### Manual Testing (Swagger UI)

1. Navigate to `/swagger`
2. Use the login form in Swagger UI
3. Login creates a `SessionId` cookie
4. All authenticated endpoints automatically use the session
5. Logout deletes the session and clears the cookie

## Future Enhancements

1. **Session Cleanup Job** - Background service to clean up expired sessions
2. **Multi-Session Support** - Allow users to have multiple sessions (different devices)
3. **Session Management UI** - Allow users to view and revoke their active sessions
4. **Session Analytics** - Track session duration, device types, locations

## Troubleshooting

### Common Issues

**Issue:** Authenticated requests return 401 Unauthorized

**Solutions:**
- Verify `SessionId` cookie is being sent with requests
- Check that session exists in `UserSessions` table
- Ensure `SessionAuthenticationMiddleware` is registered before `UseAuthentication()`
- Verify JWT token in session hasn't expired

**Issue:** Session not created after login

**Solutions:**
- Check `IdentityService.SignIn()` is calling `sessionService.CreateSession()`
- Verify `SetSessionCookie()` is setting the cookie correctly
- Ensure database connection is working

**Issue:** Tokens not being refreshed

**Solutions:**
- Verify `RefreshToken` endpoint is updating the session
- Check that old session is being found by `UserId` and `IP`
- Ensure new tokens are being saved to database

## Conclusion

The session-based authentication architecture provides:

? **Enhanced Security** - Tokens never exposed to client  
? **Transparent to Controllers** - No code changes needed  
? **Server-Side Control** - Sessions can be revoked remotely  
? **Better Audit Trail** - All sessions tracked in database  
? **CSRF Protection** - SameSite cookie policy  
? **XSS Protection** - HTTP-only cookies  

All controllers with authentication attributes (`[AuthorizeAdministrator]`, `[AuthorizeTenantAdministrator]`) automatically benefit from session-based authentication without any code changes.
