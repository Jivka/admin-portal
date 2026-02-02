# AP (Admin Portal) Solution

A comprehensive .NET 9 identity and user management platform built with ASP.NET Core Razor Pages and Web API.

## Overview

The AP solution is a multi-tenant admin portal system that provides robust identity management, authentication, and user administration capabilities. The solution is architected using a modular approach with clear separation of concerns across multiple projects.

## Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Razor Pages** - Web UI framework
- **Entity Framework Core** - ORM with SQL Server provider
- **JWT Authentication** - Token-based authentication
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
- Swagger API documentation with authentication

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
  - `IdentityService` - Core identity operations
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
  - **Middleware**: JWT authentication, Swagger auth, global exception handling
  - **Attributes**: Authorization, validation (Password strength, Email domain)
  - **Extensions**: Service collection, application builder, object, string, datetime, JSON
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
     "JwtTokenTTE": 1,
     "RefreshTokenTTE": 7
   }
   ```

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
- JWT-based API authentication
- Cookie-based web authentication
- Refresh token mechanism
- Role-based access control (RBAC)
- System admin and tenant admin roles

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
- Comprehensive Swagger documentation
- Standardized response format with `ApiResult<T>`
- Global exception handling
- Request/response logging with Serilog

### Security
- Password strength validation
- Email domain validation
- BCrypt password hashing
- JWT token expiration and refresh
- Token revocation
- HTTPS enforcement

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
- Review and adjust password policy settings

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
