# Admin Portal

A full-stack multi-tenant admin portal for identity and user management with session-based authentication.

## Overview

The Admin Portal (AP) is a comprehensive solution combining a **React 19 frontend** with a **.NET 9 backend** to provide secure, scalable identity and user management capabilities. Built with modern technologies and best practices, it features session-based authentication, role-based access control, multi-tenancy support, and a type-safe API integration layer.

## Key Features

✅ **Session-Based Authentication** - Secure server-side JWT storage with HTTP-only cookies  
✅ **Multi-Tenant Architecture** - Complete tenant isolation and management  
✅ **Role-Based Access Control (RBAC)** - Granular permission system  
✅ **Type-Safe API** - Auto-generated TypeScript types from OpenAPI/Swagger  
✅ **Modern React UI** - Material-UI components with Redux state management  
✅ **RESTful API** - Comprehensive Web API with Swagger documentation  
✅ **Email Integration** - SMTP-based email for verification and notifications  
✅ **Secure Password Management** - BCrypt hashing with strength validation  
✅ **Token Refresh** - Automatic token renewal with session rotation  
✅ **Audit Trail** - Entity tracking with creation/modification timestamps  

## Architecture

### Frontend (React + TypeScript)
- **Framework**: React 19 with TypeScript
- **Build Tool**: Vite for fast development and optimized builds
- **State Management**: Redux Toolkit with typed hooks
- **UI Framework**: Material-UI (MUI) with custom theming
- **Routing**: React Router v7 with route guards
- **HTTP Client**: Axios with automatic token refresh
- **Type Safety**: OpenAPI-generated types from backend

### Backend (.NET 9)
- **Platform**: ASP.NET Core Web API + Razor Pages
- **Authentication**: Session-based with JWT stored server-side
- **ORM**: Entity Framework Core with SQL Server
- **Logging**: Serilog with structured logging
- **Testing**: xUnit with Moq for unit tests
- **Documentation**: Swagger/OpenAPI with interactive UI
- **Messaging**: MassTransit for event-driven communication
- **Email**: MailKit/MimeKit for SMTP integration

## Project Structure

```
admin-portal/
├── client/                     # React frontend application
│   ├── src/
│   │   ├── api/               # API client and endpoints
│   │   ├── components/        # Reusable React components
│   │   ├── features/          # Feature-based modules
│   │   ├── routes/            # Route configuration with guards
│   │   ├── store/             # Redux state management
│   │   ├── types/             # TypeScript types (auto-generated)
│   │   └── utils/             # Helper functions and constants
│   ├── package.json
│   ├── vite.config.ts
│   └── README.md              # Frontend documentation
│
├── server/                     # .NET backend application
│   ├── AP.Platform/           # Main web application host
│   ├── AP.Identity.Internal/  # Identity business logic
│   ├── AP.Common/             # Shared utilities and middleware
│   ├── AP.Common.Data/        # Entity Framework data layer
│   ├── AP.Identity.Internal.Tests/  # Unit tests
│   ├── AP.sln                 # Visual Studio solution
│   └── README.md              # Backend documentation
│
└── README.md                   # This file
```

## Quick Start

### Prerequisites

**Backend**:
- .NET 9 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022, Rider, or VS Code with C# extension

**Frontend**:
- Node.js 18.x or later
- npm or yarn

### Installation & Setup

#### 1. Clone the Repository
```bash
git clone <repository-url>
cd admin-portal
```

#### 2. Backend Setup

**Configure Database**:

Edit `server/AP.Platform/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "APConnection": "Server=localhost\\SQLEXPRESS;Database=AP;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Configure Authentication**:

Update `IdentitySettings` in `appsettings.json`:
```json
{
  "IdentitySettings": {
    "Secret": "your-secure-jwt-secret-key-min-32-chars",
    "EmailDomain": "@yourdomain.com",
    "MinPasswordLength": 8,
    "MaxPasswordLength": 16,
    "JwtTokenTTE": 1,
    "RefreshTokenTTE": 7
  }
}
```

**Apply Migrations & Run**:
```bash
cd server/AP.Platform
dotnet restore
dotnet ef database update --project ../AP.Common.Data
dotnet run
```

Backend will be available at: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

#### 3. Frontend Setup

**Install Dependencies**:
```bash
cd client
npm install
```

**Configure Environment**:

Create `client/.env`:
```env
VITE_API_URL=https://localhost:5001
```

**Generate Types & Run**:
```bash
npm run generate-types
npm run dev
```

Frontend will be available at: `http://localhost:5173`

## Authentication Architecture

This application uses a **session-based authentication** approach for maximum security:

### How It Works

1. **Login** (`POST /identity/sign-in`):
   - User submits credentials
   - Server validates and generates JWT access + refresh tokens
   - Tokens stored in `UserSessions` table (server-side)
   - Only `SessionId` sent to client as HTTP-only cookie
   - **JWT tokens never exposed to client**

2. **Authenticated Requests**:
   - Browser automatically includes `SessionId` cookie
   - `SessionAuthenticationMiddleware` retrieves session from database
   - JWT extracted from session and injected into `Authorization` header
   - Standard JWT Bearer authentication validates token
   - Controller receives authenticated request

3. **Token Refresh** (`POST /identity/refresh-token`):
   - Client sends request with `SessionId` cookie
   - Server validates refresh token from session
   - New access and refresh tokens generated
   - Session updated in database with new tokens
   - Same `SessionId` cookie retained (session rotated, not recreated)

4. **Logout** (`POST /identity/logout`):
   - Session deleted from database
   - Cookie cleared from client

### Security Benefits

- ✅ **XSS Protection**: JWT tokens never in JavaScript scope
- ✅ **CSRF Protection**: SameSite=Strict cookie policy
- ✅ **Token Theft Prevention**: Tokens stored server-side only
- ✅ **Instant Revocation**: Delete session to invalidate immediately
- ✅ **HTTPS Enforcement**: Secure flag on cookies

For detailed architecture documentation, see:
- [server/AUTHENTICATION_ARCHITECTURE.md](server/AUTHENTICATION_ARCHITECTURE.md)
- [server/QUICK_REFERENCE.md](server/QUICK_REFERENCE.md)

## Development Workflows

### Backend Development

**Run with environment**:
```bash
cd server/AP.Platform
dotnet run -e "Local"
```

**Run tests**:
```bash
cd server/AP.Identity.Internal.Tests
dotnet test
```

**Add migration**:
```bash
cd server/AP.Common.Data
dotnet ef migrations add MigrationName --startup-project ../AP.Platform
dotnet ef database update --startup-project ../AP.Platform
```

**Update Swagger JSON** (after API changes):
```bash
# While server is running
curl https://localhost:5001/swagger/v1/swagger.json -o server/swagger.json
```

### Frontend Development

**Start dev server**:
```bash
cd client
npm run dev
```

**Regenerate API types** (after backend changes):
```bash
npm run generate-types
```

**Build for production**:
```bash
npm run build
npm run preview  # Preview production build
```

**Linting**:
```bash
npm run lint
```

## API Integration

### Frontend → Backend Communication

The frontend uses a configured Axios client (`src/api/client.ts`) that:
- Includes `withCredentials: true` for session cookies
- Automatically refreshes tokens on 401 responses
- Proxies requests through Vite dev server (development)
- Uses type-safe request/response models

**Example API Call**:
```typescript
import { apiClient } from '../api/client';
import type { SigninRequest, UserOutput } from '../types';

const credentials: SigninRequest = {
  email: 'user@example.com',
  password: 'password123',
};

const response = await apiClient.post<UserOutput>('/identity/sign-in', credentials);
```

### Type Safety

TypeScript types are auto-generated from the backend Swagger spec:

1. Backend generates `swagger.json` when running
2. Frontend runs `npm run generate-types`
3. Types created in `client/src/types/api.ts`
4. Import and use in components

This ensures **compile-time validation** of API contracts.

## Routing & Authorization

### Frontend Route Guards

**PrivateRoute**: Requires authentication
```typescript
<PrivateRoute>
  <Dashboard />
</PrivateRoute>
```

**RoleRoute**: Requires specific role
```typescript
<RoleRoute allowedRoleIds={[1]}>  {/* System Admin only */}
  <SystemSettings />
</RoleRoute>
```

### Role IDs
- `1` - System Administrator
- `2` - Tenant Administrator

### Backend Authorization

Controllers use attributes for authorization:

```csharp
[Authorize]  // Requires authentication
public class UsersController : ControllerBase
{
    [Authorize(Roles = "SystemAdmin")]  // Requires System Admin role
    public async Task<ActionResult<ApiResult<List<UserOutput>>>> GetAllUsers()
    { ... }
}
```

## Configuration

### Backend (`server/AP.Platform/appsettings.json`)

```json
{
  "IdentitySettings": {
    "Secret": "32+ character secret key",
    "EmailDomain": "@company.com",
    "MinPasswordLength": 8,
    "MaxPasswordLength": 16,
    "JwtTokenTTE": 1,          // Access token lifetime (days)
    "RefreshTokenTTE": 7,      // Refresh token lifetime (days)
    "RefreshTokenTTL": 2,      // Token refresh window (minutes)
    "ResetTokenTTE": 1         // Password reset token lifetime (days)
  },
  "ConnectionStrings": {
    "APConnection": "Server=...;Database=AP;..."
  },
  "EmailSettings": {
    "SmtpHost": "smtp.mailprovider.com",
    "SmtpPort": 587,
    "SmtpUser": "username",
    "SmtpPass": "password",
    "EmailFrom": "noreply@company.com"
  }
}
```

### Frontend (`client/.env`)

```env
VITE_API_URL=https://localhost:5001
```

## Database Schema

Key entities:
- **User**: User accounts with credentials and profile
- **Role**: System and tenant roles
- **Tenant**: Multi-tenant organization records
- **UserSession**: Server-side session storage with JWT tokens
- **UserTenant**: User-tenant associations
- **RefreshToken**: Token refresh tracking
- **UserEvent**: Audit trail for user actions

All entities implement `IAuditable` interface for automatic tracking:
- `CreatedOn`, `CreatedBy`
- `ModifiedOn`, `ModifiedBy`

## Testing

### Backend Tests

```bash
cd server/AP.Identity.Internal.Tests
dotnet test
```

Test structure:
- `Controllers/` - Controller unit tests
- Uses xUnit + Moq
- Mocks for services and data context

### Frontend Tests

Currently no test framework configured. Recommended additions:
- Vitest for unit/integration tests
- React Testing Library for component tests
- MSW for API mocking

## Deployment Considerations

### Security Checklist

- [ ] Change JWT `Secret` to secure random value (min 32 chars)
- [ ] Store secrets in Azure Key Vault or User Secrets (not appsettings.json)
- [ ] Enable HTTPS enforcement in production
- [ ] Configure appropriate CORS policies
- [ ] Review and adjust password policy settings
- [ ] Set `SameSite=Strict` and `Secure` flags on cookies
- [ ] Configure proper SMTP credentials
- [ ] Review and limit API rate limiting
- [ ] Enable Application Insights for monitoring

### Environment-Specific Settings

Use environment-specific config files:
- `appsettings.Development.json`
- `appsettings.Production.json`
- `appsettings.Local.json` (local overrides, git-ignored)

## Common Tasks

### Reset Database
```bash
cd server/AP.Platform
dotnet ef database drop --project ../AP.Common.Data
dotnet ef database update --project ../AP.Common.Data
```

### Add New API Endpoint
1. Create endpoint in backend controller
2. Run backend to update Swagger
3. Export swagger.json: `curl https://localhost:5001/swagger/v1/swagger.json -o server/swagger.json`
4. Regenerate frontend types: `cd client && npm run generate-types`
5. Use typed request/response in frontend

### Add New Frontend Route
1. Create component in `client/src/features/`
2. Add route in `client/src/routes/index.tsx`
3. Wrap with `<PrivateRoute>` or `<RoleRoute>` as needed

### Update Password Policy
Edit `IdentitySettings` in `appsettings.json`:
```json
{
  "MinPasswordLength": 10,
  "MaxPasswordLength": 20,
  "PasswordSpecialCharacters": "!@#$%^&*()_+"
}
```

## Troubleshooting

### Backend Issues

**Migration errors**:
```bash
# Ensure you're in the correct directory
cd server/AP.Platform
dotnet ef database update --project ../AP.Common.Data --verbose
```

**Port conflicts**:
Edit `server/AP.Platform/Properties/launchSettings.json` to change ports.

**Session authentication not working**:
- Check `SessionAuthenticationMiddleware` is registered before `UseAuthentication()`
- Verify CORS allows credentials: `AllowCredentials()`
- Ensure cookies have `SameSite=None` for cross-origin (dev) or `SameSite=Strict` (production)

### Frontend Issues

**Type errors after API changes**:
```bash
cd client
npm run generate-types
```

**CORS errors**:
- Verify Vite proxy in `vite.config.ts`
- Check backend CORS configuration allows frontend origin
- Ensure backend `AllowCredentials()` is set

**Authentication loop**:
- Clear browser cookies
- Check Redux state in browser DevTools
- Verify `/identity/refresh-token` endpoint is working

## Documentation

- [Backend README](server/README.md) - Detailed .NET solution documentation
- [Frontend README](client/README.md) - React application guide
- [Authentication Architecture](server/AUTHENTICATION_ARCHITECTURE.md) - Session-based auth deep dive
- [Quick Reference](server/QUICK_REFERENCE.md) - API usage examples and Swagger guide

## Tech Stack Summary

| Layer | Technology |
|-------|-----------|
| Frontend Framework | React 19 |
| Frontend Language | TypeScript |
| Frontend Build | Vite |
| Frontend State | Redux Toolkit |
| Frontend UI | Material-UI (MUI) |
| Frontend Routing | React Router v7 |
| Frontend HTTP | Axios |
| Backend Framework | .NET 9 / ASP.NET Core |
| Backend Language | C# |
| Backend ORM | Entity Framework Core |
| Backend Auth | JWT with Session Storage |
| Backend API Docs | Swagger/OpenAPI |
| Backend Testing | xUnit + Moq |
| Backend Logging | Serilog |
| Backend Email | MailKit/MimeKit |
| Database | SQL Server |
| Messaging | MassTransit |
| Type Generation | openapi-typescript |

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

[Specify your license here]

## Support

For questions or issues:
- Check [Troubleshooting](#troubleshooting) section
- Review [Documentation](#documentation) links
- Open an issue in the repository

---

**Built with ❤️ using React and .NET**
