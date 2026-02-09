# Swagger Session-Based Authentication Update

## Overview
Updated Swagger UI to work with the new session-based authentication system. All API requests from Swagger now use only the SessionId cookie for authentication, with JWT tokens managed entirely on the server side.

## Changes Made

### 1. Updated `AP.Platform\wwwroot\swagger\swagger.js`

#### Modified Login Function
- Updated comment to reflect session-based authentication
- Changed authorization placeholder text to indicate "Session-based (SessionId cookie)"
- Added call to `addLogoutButton()` after successful login

#### Added Logout Functionality
Two new functions were added:

**`addLogoutButton()`**
- Creates a logout button in the Swagger UI
- Positioned below the login area with appropriate styling
- Red background color to distinguish it from login
- Only appears after successful authentication

**`logout()`**
- Sends POST request to `/identity/logout` endpoint
- Includes credentials to send SessionId cookie
- Resets the global login state flag
- Clears Swagger authorization state
- Reloads the page to show login UI again

### 2. Updated `AP.Common\Utilities\Extensions\ServiceCollectionExtensions.cs`

#### Modified Swagger Security Description
Changed the Bearer scheme description from:
```
"Authorization header using the Bearer scheme. Example: \"bearer {token}\""
```

To:
```
"Session-based authentication using SessionId cookie. The JWT token is retrieved from the server-side session automatically. Login using the Swagger UI login form to authenticate."
```

This clearly communicates to API consumers that:
- Authentication is session-based
- SessionId cookie is used automatically
- No need to manually provide JWT tokens
- Use the Swagger login form for authentication

## How It Works

### User Login Flow in Swagger

1. **User Opens Swagger**
   - Swagger UI loads at `/swagger`
   - Custom login form appears (email/password fields)
   - Login button is visible

2. **User Enters Credentials**
   - User fills in email and password
   - Clicks "Login" button

3. **Login Request**
   ```javascript
   POST /identity/sign-in
   Content-Type: application/json
   Credentials: include
   
   { "Email": "user@example.com", "Password": "password" }
   ```

4. **Server Response**
   - Server validates credentials
   - Creates server-side session in UserSessions table
   - Sets SessionId cookie in response
   - Returns user information

5. **Swagger UI Updates**
   - Login form is removed
   - Logout button appears
   - Swagger authorization state is set
   - Global login flag is set to prevent re-showing login

### API Request Flow

1. **User Invokes API Endpoint**
   - User clicks "Try it out" on any endpoint
   - Enters parameters and clicks "Execute"

2. **Request Preparation**
   - Swagger fetch function intercepts request
   - Sets `credentials: 'include'` to send cookies
   - SessionId cookie is automatically included

3. **Server Processing**
   ```
   Request ? SessionAuthenticationMiddleware
           ? Reads SessionId from cookie
           ? Queries UserSessions table
           ? Retrieves JWT access token
           ? Injects token into Authorization header
           ? Proceeds to JWT authentication
           ? Reaches controller
   ```

4. **Response**
   - Server processes request with authenticated user context
   - Returns response to Swagger UI
   - SessionId cookie remains valid for subsequent requests

### Logout Flow

1. **User Clicks Logout**
   - Logout button is clicked in Swagger UI

2. **Logout Request**
   ```javascript
   POST /identity/logout
   Credentials: include
   ```

3. **Server Processing**
   - Reads SessionId from cookie
   - Deletes session from UserSessions table
   - Clears SessionId cookie

4. **Swagger UI Reset**
   - Login state flag is reset
   - Swagger authorization is cleared
   - Page reloads to show login form again

## Cookie Configuration

### SessionId Cookie Properties
```javascript
{
    name: "SessionId",
    value: "{sessionId}",  // e.g., "123"
    httpOnly: true,         // Not accessible via JavaScript
    secure: true,           // HTTPS only
    sameSite: "Strict",     // CSRF protection
    expires: "7 days"       // Based on RefreshTokenTTE setting
}
```

## Security Benefits

### What's Sent to Client
- **Before**: JWT access token (large, contains claims) + Refresh token
- **After**: SessionId only (small integer, meaningless without database)

### Attack Surface Reduction
- **XSS Protection**: Even if XSS vulnerability exists, attacker only gets session ID
- **Token Theft**: JWT tokens never leave the server, can't be stolen from client
- **Session Control**: Server can invalidate sessions instantly by deleting from database

### Network Traffic
- **Reduced Payload**: Session ID is much smaller than JWT token
- **Same Security**: HTTP-only, Secure, SameSite flags protect the cookie

## Testing the Swagger Integration

### Manual Testing Steps

1. **Test Login**
   ```
   1. Navigate to https://localhost:7292/swagger
   2. Enter valid credentials in login form
   3. Click Login
   4. Verify login form disappears
   5. Verify logout button appears
   6. Check browser cookies for SessionId
   ```

2. **Test API Calls**
   ```
   1. After logging in, try any protected endpoint
   2. Click "Try it out"
   3. Click "Execute"
   4. Verify successful response (200 OK)
   5. Check Network tab: SessionId cookie sent
   6. Verify no JWT in request headers from client side
   ```

3. **Test Logout**
   ```
   1. After logging in, click Logout button
   2. Verify page reloads
   3. Verify login form reappears
   4. Verify SessionId cookie is deleted
   5. Try API call - should get 401 Unauthorized
   ```

4. **Test Session Persistence**
   ```
   1. Login to Swagger
   2. Make API calls
   3. Refresh the page (F5)
   4. SessionId cookie should persist
   5. Page should remember logged-in state
   6. API calls should still work
   ```

### Browser DevTools Inspection

#### Check Cookies (Application/Storage tab)
```
Name: SessionId
Value: 123
Domain: localhost
Path: /
Expires: (7 days from now)
HttpOnly: ?
Secure: ?
SameSite: Strict
```

#### Check Network Requests
```
Request Headers:
  Cookie: SessionId=123
  (No Authorization header from client)

Response Headers:
  (Server doesn't send JWT to client)
```

## Troubleshooting

### Issue: Login form doesn't disappear
**Cause**: Login request failed or SessionId cookie not set
**Solution**: 
- Check browser console for errors
- Verify `/identity/sign-in` returns 200 OK
- Check if SessionId cookie is set in Application tab

### Issue: API calls return 401 Unauthorized
**Cause**: Session not found or expired
**Solution**:
- Verify SessionId cookie exists and has correct value
- Check UserSessions table for matching session
- Try logging out and logging in again

### Issue: Logout doesn't work
**Cause**: Logout endpoint not reachable or session already deleted
**Solution**:
- Check browser console for errors
- Verify `/identity/logout` endpoint is accessible
- Ensure SessionId cookie is being sent with logout request

### Issue: Swagger UI shows both login form and logout button
**Cause**: Login state not properly managed
**Solution**:
- Refresh the page completely
- Clear browser cache
- Check `window._swaggerLoginState.isLoggedIn` in console

## Future Enhancements

### Possible Improvements

1. **Session Expiry Warning**
   - Add UI notification when session is about to expire
   - Prompt user to refresh session or re-login

2. **Multiple Sessions**
   - Show list of active sessions
   - Allow user to manage/revoke other sessions

3. **Remember Me**
   - Option to extend session duration
   - Create longer-lived sessions for trusted devices

4. **Session Info Display**
   - Show current session ID
   - Display session creation time and expiry
   - Show user info from session

## Comparison: Before vs After

### Before (Cookie-based JWT)
```
Client Request:
  Cookie: Authentication=eyJhbGc...{long JWT}
  Cookie: RefreshToken=a1b2c3...{long token}

Server Process:
  ? Read JWT from cookie
  ? Validate JWT
  ? Extract claims
  ? Authenticate user
```

### After (Session-based)
```
Client Request:
  Cookie: SessionId=123

Server Process:
  ? Read SessionId from cookie
  ? Query UserSessions table (SessionId=123)
  ? Retrieve JWT from session
  ? Inject JWT into Authorization header
  ? Validate JWT
  ? Extract claims
  ? Authenticate user
```

### Key Differences

| Aspect | Before | After |
|--------|--------|-------|
| Cookie Size | ~2-4 KB (JWT) | ~10 bytes (ID) |
| Client Storage | JWT + Refresh Token | Session ID only |
| Token Visibility | In cookies (HTTP-only) | Never sent to client |
| Session Control | Limited | Full server control |
| Invalidation | Wait for expiry | Instant (delete session) |
| Multi-device | Complex | Simple (one session per device) |

## Conclusion

The Swagger UI now seamlessly integrates with the session-based authentication system. Users experience a clean login/logout flow, and all API requests automatically include the SessionId cookie. The server handles all JWT token management, providing enhanced security and better session control.
