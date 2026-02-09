# AP (Admin Portal) Solution

A comprehensive .NET 9 identity and user management platform built with ASP.NET Core Razor Pages and Web API.

## Overview

The AP solution is a multi-tenant admin portal system that provides robust identity management, authentication, and user administration capabilities. The solution is architected using a modular approach with clear separation of concerns across multiple projects.

## Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Razor Pages** - Web UI framework
- **Entity Framework Core** - ORM with SQL Server provider
- **JWT Authentication** - Token-based authentication with cookie storage
- **Cookie Authentication** - Secure HTTP-only cookies for web and API
- **BCrypt.Net** - Password hashing
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for tests
- **MassTransit** - Message-based communication
- **MailKit/MimeKit** - Email functionality
- **Azure Integration** - Azure Key Vault, App Configuration

## Solution Structure

### 1. AP.Platform
**Type**: ASP.NET Core Web Application (Razor Pages)  
**Purpose**: Main web application and API host

- Razor Pages for user-facing web interface (Login, Signup, Password Reset, etc.)
- Hosts REST API controllers
- Implements authentication middleware (JWT, Cookies)
- Swagger UI for API documentation
- Session management
- CORS configuration

**Key Features**:
- User authentication flows (Sign in, Sign up, Logout)
- Password management (Forgot, Reset, Change)
- Email verification
- Cookie-based authentication for web and API
- Swagger API documentation with cookie authentication support

### 2. AP.Identity.Internal
**Type**: Class Library  
**Purpose**: Core identity and user management business logic

**Components**:
- **Controllers**: API endpoints for identity, users, tenants, and authentication
  - `IdentityController` - Authentication operations
  - `UsersController` - User management
  - `TenantUsersController` - Tenant-specific user operations
  - `TenantsController` - Tenant management
  - `LoginController` - Login operations

- **Services**: Business logic implementation
- `IdentityService` - Core identity operations with cookie management
- `UsersService` - User CRUD and management
- `TenantsService` - Tenant operations
- `JwtService` - JWT token generation and validation
- `RefreshTokenService` - Token refresh logic
- `SystemService` - System-level operations

- **Processors**: Background/async operations
  - `UserEventProcessor` - User event handling
  - `SendEmailProcessor` - Email sending operations

- **Models**: Request/response DTOs for all operations

**Key Features**:
- Multi-tenant user management
- Role-based access control (RBAC)
- JWT token generation and refresh
- Password encryption with BCrypt
- User activation/deactivation
- Email verification workflows

### 3. AP.Common
**Type**: Class Library  
**Purpose**: Shared utilities, services, and infrastructure

**Components**:
- **Services**:
  - `EmailService` - Email sending functionality
  - `CacheService` - In-memory caching
  - `CurrentUser` - Current user context
  - `CurrentToken` - Current token context

- **Utilities**:
- **Middleware**: JWT cookie authentication, JWT header authentication, Swagger auth, global exception handling
- **Attributes**: Authorization, validation (Password strength, Email domain)
- **Extensions**: Service collection (with cookie and token authentication), application builder, object, string, datetime, JSON
- **Converters**: JSON converters for enums and decimals
- **Helpers**: Pagination, ordering

- **Models**:
  - `ApiResult<T>` - Standardized API response wrapper
  - `ApiError` - Error response structure
  - `TenantRole` - Tenant role mapping

**Key Features**:
- Reusable middleware components
- Custom validation attributes
- Extension methods for common operations
- Standardized API response patterns
- Azure Key Vault integration

### 4. AP.Common.Data
**Type**: Class Library  
**Purpose**: Data access layer and Entity Framework Core implementation

**Components**:
- **DataContext**: Main EF Core DbContext
- **Entities**: Database entity models
  - User, Role, Tenant, Contact
  - UserSession, UserEvent, UserTenant
  - RefreshToken, TenantContact
  - TenantType, TenantOwnership

- **Enums**: 
  - `Roles` - System roles
  - `TenantTypes` - Tenant classifications
  - `TenantOwnerships` - Ownership models

- **DataSeeder**: Initial database seeding
- **Options**: Configuration models (IdentitySettings, EmailSettings)

**Key Features**:
- Entity Framework Core with SQL Server
- Database migrations support
- Audit trail with IAuditable interface
- Data seeding for initial setup
- Fluent API configuration

### 5. AP.Identity.Internal.Tests
**Type**: xUnit Test Project  
**Purpose**: Unit tests for identity module

**Test Coverage**:
- `UsersControllerTests` - User management endpoints
- `IdentityControllerTests` - Authentication endpoints

**Testing Stack**:
- xUnit for test framework
- Moq for mocking dependencies
- Test data builders and fixtures

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or JetBrains Rider (or VS Code with C# extension)

### Configuration

1. **Database Connection**:
   Update the connection string in `AP.Platform/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "APConnection": "Server=localhost\\SQLEXPRESS;Database=AP;..."
   }
   ```

2. **Identity Settings**:
Configure authentication settings:
```json
"IdentitySettings": {
  "Secret": "your-jwt-secret-key",
  "EmailDomain": "@yourdomain.com",
  "MinPasswordLength": 6,
  "MaxPasswordLength": 16,
  "JwtTokenTTE": 1,        // JWT access token expiration in days (also used for cookie expiration)
  "RefreshTokenTTE": 7     // Refresh token expiration in days
}
```
   
**Note**: The `JwtTokenTTE` value determines how long the authentication cookie remains valid. 
The `RefreshTokenTTE` determines the refresh token cookie expiration.

3. **Email Settings**:
   Configure SMTP settings for email functionality:
   ```json
   "EmailSettings": {
     "SmtpHost": "smtp.yourprovider.com",
     "SmtpPort": 587,
     "SmtpUser": "your-smtp-user",
     "SmtpPass": "your-smtp-password",
     "EmailFrom": "noreply@yourdomain.com"
   }
   ```

### Running the Application

1. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

2. **Apply Database Migrations**:
   ```bash
   cd AP.Common.Data
   dotnet ef database update --startup-project ../AP.Platform
   ```

3. **Run the Application**:
   ```bash
   cd AP.Platform
   dotnet run
   ```

4. **Access the Application**:
   - Web UI: `https://localhost:7292` (or configured port)
   - Swagger UI: `https://localhost:7292/swagger`

### Running Tests

```bash
cd AP.Identity.Internal.Tests
dotnet test
```

## Project Dependencies

```
AP.Platform
??? AP.Identity.Internal
?   ??? AP.Common
?       ??? AP.Common.Data
??? AP.Common
    ??? AP.Common.Data

AP.Identity.Internal.Tests
??? AP.Identity.Internal
```

## Key Features

### Authentication & Authorization
- **Session-Based Authentication (Primary)**:
  - All authenticated controllers read SessionId from cookie
  - JWT access and refresh tokens retrieved from server-side session
  - SessionAuthenticationMiddleware automatically handles token extraction
  - Tokens never exposed to client - stored securely on server
- **Fallback Authentication Support**:
  - JWT Bearer token support via Authorization header (for API clients)
  - Direct header-based authentication for programmatic access
- **Secure Cookie Storage**:
  - HTTP-only SessionId cookie (prevents XSS attacks)
  - Secure flag enforced (HTTPS only)
  - SameSite strict policy (prevents CSRF attacks)
  - Session cookie expires based on RefreshTokenTTE setting
- Automatic token extraction from session for all authenticated requests
- Refresh token mechanism with automatic session rotation
- Role-based access control (RBAC)
- System admin and tenant admin authorization attributes

### User Management
- Create, read, update, delete users
- User activation/deactivation
- Password management (change, reset, forgot)
- Email verification
- User event tracking

### Multi-Tenancy
- Tenant creation and management
- Tenant-specific user assignments
- Tenant contact management
- Tenant ownership and type classification

### API Features
- RESTful API design
- Session-based authentication with secure server-side token storage
- Comprehensive Swagger documentation with session authentication
- Custom Swagger UI with login interface
- Standardized response format with `ApiResult<T>`
- Global exception handling
- Request/response logging with Serilog
- Automatic session credential handling for authenticated requests

### Security
- Password strength validation
- Email domain validation
- BCrypt password hashing
- JWT token expiration and refresh
- Token revocation
- HTTPS enforcement
- **Session-Based Security**:
  - SessionId stored in HTTP-only, Secure, SameSite=Strict cookie
  - JWT tokens stored server-side in database (never exposed to client)
  - Access tokens expire based on `JwtTokenTTE` setting (default: 1 day)
  - Session cookie expires based on `RefreshTokenTTE` setting (default: 7 days)
  - Automatic session cleanup for expired sessions
  - Protected against XSS, CSRF, and token theft attacks
- **Token Management**:
  - JWT tokens automatically retrieved from session for API authentication
  - SessionAuthenticationMiddleware handles token extraction transparently
  - Both header-based (Bearer token) and session-based authentication supported
  - Swagger UI uses session-based authentication after login

## Architecture Patterns

- **Dependency Injection**: Comprehensive use of DI throughout the solution
- **Repository Pattern**: Data access abstraction with EF Core
- **Service Layer**: Business logic separated from controllers
- **DTO Pattern**: Request/response models separate from entities
- **Middleware Pipeline**: Custom middleware for authentication and error handling
- **Result Pattern**: Standardized `ApiResult<T>` for operation outcomes

## Logging

The application uses Serilog for structured logging with:
- File logging (rolling daily logs in `Logs/` directory)
- Application Insights integration (for production)
- Configurable log levels

## API Documentation

API documentation is available through Swagger UI when running the application in development mode. Access it at `/swagger` endpoint.

## Security Considerations

- Sensitive configuration values should be stored in User Secrets (development) or Azure Key Vault (production)
- Update the JWT secret key before deploying to production
- Configure CORS policies appropriately for your environment
  - **Important**: CORS must allow credentials (`AllowCredentials()`) for session-based authentication to work
- Review and adjust password policy settings
- **Session-Based Authentication**:
  - Only SessionId cookie is sent to client (HTTP-only, Secure, SameSite=Strict flags)
  - JWT access and refresh tokens stored server-side in `UserSessions` table
  - Session cookie expires based on `RefreshTokenTTE` setting (default: 7 days)
  - Access tokens expire based on `JwtTokenTTE` setting (default: 1 day)
  - Ensure HTTPS is enforced in production for secure cookie transmission
  - Sessions can be invalidated server-side for immediate logout
- **Token Management**:
  - SessionAuthenticationMiddleware automatically extracts tokens from session
  - Controllers don't need to handle session logic - it's transparent
  - Both session-based (via cookie) and header-based (Bearer token) authentication supported
  - Swagger UI uses session-based authentication after login
  - All authenticated API controllers automatically work with session-based auth

## Contributing

When contributing to this project:
1. Follow the existing code style and conventions
2. Write unit tests for new functionality
3. Update documentation as needed
4. Ensure all tests pass before submitting changes

## License

[Specify your license here]

## Support

[Add contact information or support channels]
