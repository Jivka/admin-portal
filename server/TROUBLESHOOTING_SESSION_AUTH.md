# Troubleshooting Session Authentication

## Issue: "Authentication required. Please login to obtain a session."

This error occurs when the JWT Bearer authentication fails to authenticate the request even though the `SessionAuthenticationMiddleware` is retrieving the token from the session.

## Debugging Steps

### 1. Check if Session is Created After Login

After logging in via `/identity/sign-in`, check the database:

```sql
SELECT TOP 10 * FROM UserSessions ORDER BY CreatedOn DESC;
```

**Expected Result:**
- A new session record with the correct `UserId`
- `AccessToken` and `RefreshToken` fields populated
- `SessionId` matches the cookie value

**If no session:** The login process isn't creating sessions. Check `IdentityService.SignIn()`.

### 2. Check if SessionId Cookie is Being Sent

**Browser DevTools:**
1. Open DevTools ? Network tab
2. Make an authenticated request to `/api/users` or similar
3. Check the request headers for `Cookie: SessionId=<value>`

**Expected Result:**
- Cookie named `SessionId` with a numeric value
- Cookie attributes: `HttpOnly`, `Secure`, `SameSite=Strict`

**If no cookie:** 
- Check if login is setting the cookie in `IdentityService.SetSessionCookie()`
- Verify cookie settings (domain, path, secure flag)

### 3. Enable Logging to See Middleware Activity

Add this to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AP.Common.Utilities.Middleware.SessionAuthenticationMiddleware": "Debug",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

**Expected Logs:**
```
[Debug] Session 123 authenticated for user 456
[Debug] AuthenticationScheme: Bearer was successfully authenticated
```

**If you see "Session not found":**
- SessionId in cookie doesn't match database
- Session was deleted/expired

**If you see "JWT Authentication failed":**
- Token is invalid, expired, or malformed
- JWT secret doesn't match

### 4. Inspect the JWT Token

Add a temporary breakpoint or log in `SessionAuthenticationMiddleware`:

```csharp
if (session != null && !string.IsNullOrEmpty(session.AccessToken))
{
    logger?.LogInformation("Access Token: {Token}", session.AccessToken.Substring(0, 50) + "...");
    // ... rest of code
}
```

**Check:**
- Token is not null or empty
- Token looks like a valid JWT (three parts separated by dots)
- Token length is reasonable (typically 200-800 characters)

### 5. Validate JWT Token Manually

Use https://jwt.io to decode the token:

**Paste the token and check:**
- **Header:** Should be `{"alg":"HS256","typ":"JWT"}`
- **Payload:** Should contain claims like `nameid`, `email`, `role`
- **Expiration:** `exp` claim should be in the future (Unix timestamp)

**Common Issues:**
- **Token Expired:** `exp` claim is in the past
- **Invalid Signature:** Secret key doesn't match
- **Missing Claims:** Required claims not present

### 6. Check JWT Configuration

Verify `appsettings.json`:

```json
{
  "IdentitySettings": {
    "Secret": "your-secret-key-min-32-chars",
    "JwtTokenTTE": 1
  }
}
```

**Requirements:**
- `Secret` must be at least 32 characters
- Same `Secret` used for signing and validation
- `JwtTokenTTE` should be greater than 0

### 7. Verify Middleware Order in Program.cs

Check the middleware pipeline:

```csharp
app.UseHttpsRedirection();
app.UseMiddleware<SessionAuthenticationMiddleware>();  // ? Must be BEFORE
app.UseAuthentication();                               // ? JWT authentication
app.UseAuthorization();
```

**Critical:** `SessionAuthenticationMiddleware` must come **before** `UseAuthentication()`.

**If order is wrong:** Authentication will fail because JWT handler runs before session middleware adds the token.

### 8. Test with Swagger UI

1. Navigate to `/swagger`
2. Use the login form (email + password)
3. After login, check browser cookies for `SessionId`
4. Try calling an authenticated endpoint like `GET /api/users`

**Expected:**
- Login returns 200 OK
- SessionId cookie is set
- Authenticated endpoints return 200 OK with data

**If authenticated endpoints fail:**
- Check browser console for errors
- Check Network tab for Authorization header
- Verify SessionId cookie is being sent

### 9. Common Issues and Solutions

#### Issue: Token is expired
**Solution:** 
- Reduce time between login and API call
- Increase `JwtTokenTTE` in appsettings
- Use refresh token endpoint to get new tokens

#### Issue: Secret key mismatch
**Solution:**
- Ensure same secret in configuration for both signing and validation
- Restart the application after changing the secret
- Clear existing sessions and re-login

#### Issue: Session not found in database
**Solution:**
- Check database connection
- Verify `UserSessions` table exists
- Ensure session wasn't deleted by cleanup job
- Confirm SessionId cookie value matches database

#### Issue: Authorization header not being set
**Solution:**
- Add logging to `SessionAuthenticationMiddleware` to verify it's running
- Check if `context.Request.Headers.ContainsKey("Authorization")` returns true after middleware
- Verify `session.AccessToken` is not null or empty

### 10. Manual Test with Postman

**Step 1: Login**
```
POST https://localhost:7292/identity/sign-in
Content-Type: application/json

{
  "Email": "admin@yourdomain.com",
  "Password": "your-password"
}
```

**Step 2: Copy SessionId from response cookies**

**Step 3: Test authenticated endpoint**
```
GET https://localhost:7292/api/users
Cookie: SessionId=<value-from-step-2>
```

**Expected:** 200 OK with user list

**If 401 Unauthorized:**
- SessionId cookie not being read
- Session not in database
- Token expired or invalid

### 11. Check Database Session Content

Query the session to verify tokens:

```sql
SELECT 
    SessionId,
    UserId,
    LEFT(AccessToken, 50) as TokenPreview,
    CreatedOn,
    CreatedFomIp
FROM UserSessions
WHERE SessionId = <your-session-id>;
```

**Verify:**
- `AccessToken` is not null
- `AccessToken` starts with `eyJ` (JWT format)
- `CreatedOn` is recent
- `UserId` matches the logged-in user

### 12. Add Detailed Logging

Temporarily add this to `SessionAuthenticationMiddleware`:

```csharp
logger?.LogInformation("SessionAuthenticationMiddleware: SessionId cookie = {Cookie}", sessionIdCookie ?? "NULL");
logger?.LogInformation("SessionAuthenticationMiddleware: Session found = {Found}", session != null);
logger?.LogInformation("SessionAuthenticationMiddleware: Has AccessToken = {HasToken}", session?.AccessToken != null);
logger?.LogInformation("SessionAuthenticationMiddleware: Authorization header set = {HeaderSet}", 
    context.Request.Headers.ContainsKey(AuthorizationHeaderName));
```

This will help you see exactly what's happening at each step.

## Quick Checklist

- [ ] Session created in database after login
- [ ] SessionId cookie is set and sent with requests
- [ ] `SessionAuthenticationMiddleware` runs before `UseAuthentication()`
- [ ] Token is retrieved from session successfully
- [ ] Authorization header is set with `Bearer <token>`
- [ ] JWT token is valid (not expired, correct signature)
- [ ] JWT secret matches between signing and validation
- [ ] Logging enabled to see authentication events

## Next Steps

If all checks pass but authentication still fails:

1. **Check the actual error from JWT authentication** - Enable debug logging for `Microsoft.AspNetCore.Authentication`
2. **Verify token claims** - Ensure required claims are present (nameid, email, etc.)
3. **Test with a fresh session** - Logout, clear cookies, login again
4. **Check for multiple authentication schemes** - Ensure JWT Bearer is the default scheme
5. **Verify controller authorization attributes** - Check `[AuthorizeAdministrator]` and `[AuthorizeTenantAdministrator]` work correctly

## Contact Points for Support

- Check logs in `Logs/` directory
- Review `AUTHENTICATION_ARCHITECTURE.md` for architecture details
- Verify configuration in `appsettings.json`
