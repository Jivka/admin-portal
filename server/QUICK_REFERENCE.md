# Session-Based Authentication - Quick Reference

## What Changed?

### Before
- JWT and refresh tokens stored in cookies
- Cookies sent to client: `Authentication` and `RefreshToken`
- Each request sends ~4KB of token data

### After  
- JWT and refresh tokens stored in server database
- Cookie sent to client: `SessionId` only
- Each request sends ~10 bytes of session ID

## For API Consumers

### Using Swagger UI

1. **Login**
   - Open `/swagger`
   - Enter email and password in login form
   - Click "Login"
   - SessionId cookie is set automatically

2. **Making API Calls**
   - Click "Try it out" on any endpoint
   - SessionId cookie is included automatically
   - No need to manually manage tokens

3. **Logout**
   - Click the "Logout" button
   - SessionId is cleared
   - Login form reappears

### Using Postman/Curl

1. **Login**
   ```bash
   curl -X POST https://localhost:7292/identity/sign-in \
     -H "Content-Type: application/json" \
     -d '{"Email":"user@example.com","Password":"password"}' \
     -c cookies.txt
   ```

2. **API Call**
   ```bash
   curl -X GET https://localhost:7292/api/users/1 \
     -b cookies.txt
   ```

3. **Logout**
   ```bash
   curl -X POST https://localhost:7292/identity/logout \
     -b cookies.txt
   ```

## For Developers

### Key Files

| File | Purpose |
|------|---------|
| `SessionService.cs` | Manages server-side sessions |
| `SessionAuthenticationMiddleware.cs` | Retrieves JWT from session |
| `IdentityService.cs` | Creates/updates sessions |
| `IdentityController.cs` | Login/logout endpoints |

### Request Flow

```
Client Request with SessionId cookie
    ?
SessionAuthenticationMiddleware
    ?
Query UserSessions table
    ?
Retrieve JWT token
    ?
Inject into Authorization header
    ?
JWT Authentication
    ?
Controller Action
```

### Database Table

```sql
UserSessions
??? SessionId (PK)
??? UserId (FK)
??? AccessToken (JWT)
??? RefreshToken
??? CreatedOn
??? CreatedFomIp
```

### Configuration

No changes needed to `appsettings.json`. Existing settings still apply:

```json
{
  "IdentitySettings": {
    "JwtTokenTTE": 1,        // JWT expiration (days)
    "RefreshTokenTTE": 7     // Session expiration (days)
  }
}
```

## For DevOps

### Deployment

1. **No database migration required** - UserSessions table already exists
2. **Users must re-login** after deployment
3. **Monitor UserSessions table growth**

### Maintenance

**Cleanup old sessions** (run weekly):
```sql
DELETE FROM UserSessions 
WHERE CreatedOn < DATEADD(day, -30, GETUTCDATE())
```

**Monitor active sessions**:
```sql
SELECT COUNT(*) as ActiveSessions 
FROM UserSessions 
WHERE CreatedOn > DATEADD(day, -7, GETUTCDATE())
```

## Troubleshooting

### User can't login
- Check UserSessions table for session creation
- Verify SessionId cookie is set (use browser DevTools)
- Check server logs for errors

### 401 Unauthorized errors
- Session might be expired or deleted
- Ask user to logout and login again
- Check if SessionId cookie exists

### Performance issues
- Check UserSessions table size
- Add index if needed: `CREATE INDEX IX_CreatedOn ON UserSessions(CreatedOn)`
- Consider implementing Redis cache

## Security Benefits

? JWT tokens never sent to client  
? Smaller cookie payload (99.7% reduction)  
? Instant session revocation  
? Better session management  
? Reduced attack surface for XSS  

## Documentation

- **Technical Details**: `SESSION_AUTHENTICATION_IMPLEMENTATION.md`
- **Swagger Guide**: `SWAGGER_SESSION_AUTHENTICATION.md`
- **Complete Summary**: `COMPLETE_IMPLEMENTATION_SUMMARY.md`

## Support

For issues or questions:
1. Check the documentation files listed above
2. Review the troubleshooting section
3. Check server logs and UserSessions table
4. Contact the development team

---

**Version**: 1.0  
**Last Updated**: 2024  
**Target Framework**: .NET 9
