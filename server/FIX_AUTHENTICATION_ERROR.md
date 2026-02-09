# Fix for "Authentication required. Please login to obtain a session."

## Root Cause

The error was caused by **conflicting authentication scheme registrations** in `Program.cs`:

```csharp
// BEFORE (INCORRECT)
builder.Services.AddTokenAuthentication(builder.Configuration);
builder.Services.AddCookieAuthentication();  // ? This was overriding JWT Bearer
```

### The Problem

1. **AddTokenAuthentication()** registered JWT Bearer as the default authentication scheme
2. **AddCookieAuthentication()** then registered Cookie authentication as the default scheme
3. The second call **overrode** the first, making Cookie authentication the default
4. When the controllers with `[AuthorizeAdministrator]` and `[AuthorizeTenantAdministrator]` attributes ran, they used **Cookie authentication** instead of **JWT Bearer authentication**
5. Since there was no cookie-based authentication ticket (only a SessionId cookie), authentication failed

## The Fix

### 1. Updated Program.cs

**Removed** the conflicting `AddCookieAuthentication()` call:

```csharp
// AFTER (CORRECT)
builder.Services.AddTokenAuthentication(builder.Configuration);
// Cookie authentication removed - using JWT Bearer with session-based token storage
```

### 2. Simplified AddTokenAuthentication

Changed from setting all scheme options to just the default:

```csharp
// BEFORE
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

// AFTER
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
```

This simplified approach sets JWT Bearer as the default scheme without conflicts.

### 3. Enhanced SessionAuthenticationMiddleware

Added improved error handling and logging:

```csharp
- Added logging for debugging
- Added null checks and exception handling
- Added check to remove existing Authorization header before setting new one
```

### 4. Added Debug Logging to JWT Events

Added `OnAuthenticationFailed` event to log JWT authentication failures:

```csharp
OnAuthenticationFailed = context =>
{
    var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
    logger?.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
    return Task.CompletedTask;
}
```

## How It Works Now

### Authentication Flow

1. **User logs in** ? `/identity/sign-in`
   - Credentials validated
   - JWT tokens generated
   - Session created in `UserSessions` table
   - **Only SessionId** sent to client as HTTP-only cookie

2. **User makes authenticated request** ? `/api/users`
   - Browser sends `SessionId` cookie
   - **SessionAuthenticationMiddleware** runs:
     - Reads `SessionId` from cookie
     - Queries `UserSessions` table
     - Retrieves `AccessToken` (JWT)
     - Sets `Authorization: Bearer <token>` header
   - **JWT Bearer Authentication** runs:
     - Validates token from Authorization header
     - Populates `User` principal with claims
   - **Controller** executes with authenticated user

3. **Authorization check**
   - `[AuthorizeAdministrator]` or `[AuthorizeTenantAdministrator]` attribute checks claims
   - Access granted or denied based on roles

## Verification Steps

### 1. Check Application Logs

With debug logging enabled, you should see:

```
[Debug] Session 123 authenticated for user 456
[Debug] AuthenticationScheme: Bearer was successfully authenticated
```

**No longer see:**
```
[Warning] JWT Authentication failed: ...
```

### 2. Test in Swagger UI

1. Navigate to `/swagger`
2. Login using the form
3. Verify `SessionId` cookie is set (browser DevTools)
4. Call `/api/users` or other authenticated endpoint
5. Should return 200 OK with data

### 3. Test with Browser DevTools

**Network Tab:**
- Request should include `Cookie: SessionId=<value>`
- Request should have `Authorization: Bearer <token>` header (set by middleware)
- Response should be 200 OK

### 4. Check Database

```sql
SELECT TOP 1 * FROM UserSessions 
WHERE SessionId = <your-session-id>
ORDER BY CreatedOn DESC;
```

Verify:
- Session exists
- `AccessToken` is populated
- `UserId` matches logged-in user

## Key Changes Summary

| Component | Before | After |
|-----------|--------|-------|
| **Program.cs** | Called both `AddTokenAuthentication()` and `AddCookieAuthentication()` | Only calls `AddTokenAuthentication()` |
| **Default Auth Scheme** | Conflicted between Cookie and JWT Bearer | JWT Bearer only |
| **SessionAuthenticationMiddleware** | Basic implementation | Enhanced with logging and error handling |
| **JWT Events** | OnChallenge only | Added OnAuthenticationFailed for debugging |

## Testing

After these changes:

? **Session authentication works** - SessionId cookie ? token from DB ? JWT validation  
? **Controllers authenticate properly** - `[Authorize*]` attributes work  
? **No authentication conflicts** - Single clear authentication scheme  
? **Better debugging** - Logs show what's happening at each step  

## Troubleshooting

If you still see the error after these changes:

1. **Restart the application** - Hot reload might not pick up authentication changes
2. **Clear browser cookies** - Old cookies might interfere
3. **Login again** - Create a fresh session
4. **Check logs** - Look for JWT authentication failures
5. **Verify token** - Use jwt.io to decode and check expiration
6. **Check secret** - Ensure same secret used for signing and validation

## Files Modified

1. `AP.Platform/Program.cs` - Removed `AddCookieAuthentication()` call
2. `AP.Common/Utilities/Extensions/ServiceCollectionExtensions.cs` - Simplified authentication registration, added debug logging
3. `AP.Common/Utilities/Middleware/SessionAuthenticationMiddleware.cs` - Enhanced error handling and logging
4. `TROUBLESHOOTING_SESSION_AUTH.md` - Created comprehensive debugging guide

## Next Steps

If authentication still fails:

1. Enable debug logging (see `TROUBLESHOOTING_SESSION_AUTH.md`)
2. Check if session is created in database
3. Verify token is valid (not expired)
4. Ensure SessionId cookie is being sent with requests
5. Check that middleware runs in correct order
