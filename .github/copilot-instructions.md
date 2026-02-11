# Admin Portal AI Coding Instructions

## Project Overview
Multi-tenant admin portal with React (TypeScript + Vite) frontend and .NET 9 backend. Uses **session-based authentication** where SessionId cookies reference server-stored JWT tokens—not direct cookie-based JWT.

## Architecture

### Backend (.NET 9)
- **AP.Platform**: ASP.NET Razor Pages + Web API host. Main entry point.
- **AP.Identity.Internal**: Identity business logic (auth, users, tenants). Controllers and services.
- **AP.Common**: Shared utilities, middleware, validation attributes, extensions.
- **AP.Common.Data**: EF Core data layer (entities, DbContext, migrations).
- **AP.Identity.Internal.Tests**: xUnit tests with Moq.

### Frontend (React 19 + TypeScript)
- **Vite** dev server and build tool.
- **Redux Toolkit** for state (auth, roles).
- **React Router** v7 with role-based route guards ([PrivateRoute.tsx](client/src/routes/PrivateRoute.tsx), [RoleRoute.tsx](client/src/routes/RoleRoute.tsx)).
- **Material-UI (MUI)** for components and theming.
- **Axios** client with credentials for session cookies ([client.ts](client/src/api/client.ts)).

## Critical: Session-Based Authentication

**NOT** traditional cookie authentication. The system works like this:

1. **Login** (`/identity/sign-in`): Server creates `UserSession` record with JWT tokens, returns only `SessionId` cookie (HTTP-only, Secure, SameSite=Strict).
2. **Authenticated Requests**: [SessionAuthenticationMiddleware.cs](server/AP.Common/Utilities/Middleware/SessionAuthenticationMiddleware.cs) reads `SessionId` cookie, retrieves JWT from `UserSessions` table, injects into `Authorization` header.
3. **Token Refresh** (`/identity/refresh-token`): Updates session with new tokens, same SessionId cookie.
4. **Logout** (`/identity/logout`): Deletes session from database, clears cookie.

**Key Files**:
- [SessionAuthenticationMiddleware.cs](server/AP.Common/Utilities/Middleware/SessionAuthenticationMiddleware.cs): Retrieves tokens from session
- [AUTHENTICATION_ARCHITECTURE.md](server/AUTHENTICATION_ARCHITECTURE.md): Complete flow documentation
- [client.ts](client/src/api/client.ts): Axios interceptor for token refresh (calls `/identity/refresh-token` on 401)

## Development Workflows

### Backend
```powershell
# Run server (from admin-portal/server/AP.Platform)
dotnet run

# Or with environment variable
dotnet run -e "Local"

# Run tests
cd AP.Identity.Internal.Tests
dotnet test

# Database migrations (from AP.Common.Data or AP.Platform)
dotnet ef migrations add <MigrationName> --startup-project ../AP.Platform
dotnet ef database update --startup-project ../AP.Platform
```

Configuration: [appsettings.json](server/AP.Platform/appsettings.json) (connection strings, IdentitySettings, EmailSettings).

### Frontend
```bash
# Dev server (from admin-portal/client)
npm run dev

# Type-safe API types from Swagger
npm run generate-types  # Creates src/types/api.ts from ../server/swagger.json

# Build
npm run build

# Lint
npm run lint
```

Environment: Create `.env` with `VITE_API_URL=https://localhost:5001` (or backend URL).

## Code Conventions

### Backend Patterns

**API Response Wrapper**: Use `ApiResult<T>` for all controller actions.
```csharp
// Success
return ApiResult<UserOutput>.Success(user);

// Failure
return ApiResult.Failure(ErrorMessagesConstants.UserNotFound);
```

See [ApiResult.cs](server/AP.Common/Models/ApiResult.cs) for implementation.

**Service Registration**: Extension methods in [ServiceCollectionExtensions.cs](server/AP.Common/Utilities/Extensions/ServiceCollectionExtensions.cs).
- `AddTokenAuthentication()` configures JWT Bearer auth.
- `AddDatabase<T>()` registers DbContext with connection string.
- Register module services via `AddIdentityModule()` in [IdentityModule.cs](server/AP.Identity.Internal/IdentityModule.cs).

**Middleware Order** ([Program.cs](server/AP.Platform/Program.cs#L77-L79)):
```csharp
app.UseMiddleware<SessionAuthenticationMiddleware>();  // FIRST: inject JWT
app.UseAuthentication();                               // THEN: validate JWT
app.UseAuthorization();
```

**Central Package Management**: All package versions in [Directory.Packages.props](server/Directory.Packages.props). Use `<PackageReference Include="PackageName" />` without version in .csproj files.

### Frontend Patterns

**Type-Safe API Calls**: Import types from [types/index.ts](client/src/types/index.ts) (generated from Swagger via `openapi-typescript`).
```typescript
import type { SigninRequest, UserOutput } from '../types';

const response = await apiClient.post<UserOutput>('/identity/sign-in', data);
```

**API Client**: Always use [apiClient](client/src/api/client.ts) (not raw axios). Configured with `withCredentials: true` for SessionId cookie and auto-refresh on 401.

**Route Protection**: Wrap routes with `<PrivateRoute>` (auth check) and `<RoleRoute allowedRoleIds={[1, 2]}>` (role check). See [routes/index.tsx](client/src/routes/index.tsx#L80-L85).

**State Management**: Redux slices in [store/](client/src/store/). Use typed hooks from [hooks.ts](client/src/store/hooks.ts):
```typescript
import { useAppDispatch, useAppSelector } from '../store/hooks';
```

## Integration Points

- **Swagger UI**: [https://localhost:7292/swagger](https://localhost:7292/swagger). Session auth built-in (login form in Swagger UI).
- **CORS**: Configured in [Program.cs](server/AP.Platform/Program.cs). Update for frontend origin.
- **Email**: MailKit/MimeKit in [EmailService.cs](server/AP.Common/Services/EmailService.cs). Configure SMTP in appsettings.json.
- **Database**: SQL Server. Connection string in appsettings.json. Entity models in [AP.Common.Data/Entities/](server/AP.Common.Data/Entities/).

## Testing

- Backend: xUnit + Moq. Test structure in [AP.Identity.Internal.Tests/](server/AP.Identity.Internal.Tests/).
- Frontend: No test framework configured yet (add Vitest if needed).

## Common Pitfalls

1. **Don't create new cookies for auth**—SessionId is the only auth cookie. JWT tokens live in database.
2. **Regenerate types after Swagger changes**: Run `npm run generate-types` in client/ when backend models change.
3. **Role IDs are numeric**: System Admin = 1, Tenant Admin = 2. See [routes/index.tsx](client/src/routes/index.tsx#L26-L27).
4. **API responses must use ApiResult<T>**: Controllers should return `ApiResult<T>`, not raw data.
5. **EF migrations**: Always specify `--startup-project ../AP.Platform` when running from AP.Common.Data.

## Key Documentation Files
- [AUTHENTICATION_ARCHITECTURE.md](server/AUTHENTICATION_ARCHITECTURE.md): Session auth deep dive
- [QUICK_REFERENCE.md](server/QUICK_REFERENCE.md): API usage examples (Swagger, Postman)
- [README.md](server/README.md): Complete solution structure and setup
