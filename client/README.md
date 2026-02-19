# Admin Portal - Frontend

Modern React-based admin portal frontend for multi-tenant user and identity management.

## Technology Stack

- **React 19** - Latest React with improved performance
- **TypeScript** - Type-safe development
- **Vite** - Fast build tool and dev server
- **Redux Toolkit** - State management
- **React Router v7** - Client-side routing
- **Material-UI (MUI)** - Component library and theming
- **Axios** - HTTP client with interceptors
- **OpenAPI TypeScript** - Type-safe API integration

## Project Structure

```
src/
├── api/                    # API client and endpoint definitions
│   ├── client.ts          # Configured Axios instance with interceptors
│   ├── auth.api.ts        # Authentication endpoints
│   ├── users.api.ts       # User management endpoints
│   ├── roles.api.ts       # Role endpoints
│   └── tenants.api.ts     # Tenant endpoints
├── assets/                # Static assets (images, icons)
├── components/            # Reusable components
│   ├── common/           # Generic UI components
│   └── layout/           # Layout components (header, sidebar, etc.)
├── features/             # Feature-based modules
│   ├── auth/            # Authentication (login, signup, etc.)
│   ├── dashboard/       # Dashboard views
│   ├── profile/         # User profile management
│   ├── tenants/         # Tenant management
│   └── users/           # User management
├── routes/              # Route configuration
│   ├── index.tsx        # Main route definitions
│   ├── PrivateRoute.tsx # Authentication guard
│   └── RoleRoute.tsx    # Role-based route guard
├── store/               # Redux state management
│   ├── index.ts         # Store configuration
│   ├── hooks.ts         # Typed Redux hooks
│   ├── authSlice.ts     # Authentication state
│   ├── usersSlice.ts    # Users state
│   ├── rolesSlice.ts    # Roles state
│   └── tenantsSlice.ts  # Tenants state
├── types/               # TypeScript type definitions
│   ├── api.ts          # Auto-generated from OpenAPI/Swagger
│   └── index.ts        # Custom types and re-exports
└── utils/              # Utility functions
    ├── constants.ts    # Application constants
    ├── enums.ts        # Enums and lookup values
    └── helpers.ts      # Helper functions
```

## Getting Started

### Prerequisites

- **Node.js** 18.x or later
- **npm** or **yarn**
- Backend API running (see `/server/README.md`)

### Installation

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Configure environment**:
   Create a `.env` file in the client directory:
   ```env
   VITE_API_URL=https://localhost:5001
   ```

3. **Generate type-safe API types** from backend Swagger:
   ```bash
   npm run generate-types
   ```
   This creates `src/types/api.ts` from `../server/swagger.json`.

### Development

**Start development server**:
```bash
npm run dev
```

The app will be available at `http://localhost:5173`

**Available scripts**:
- `npm run dev` - Start Vite dev server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint
- `npm run generate-types` - Generate TypeScript types from Swagger

## Authentication

The application uses **session-based authentication** with HTTP-only cookies:

### How It Works

1. **Login**: User credentials sent to `/identity/sign-in`
   - Server creates session and returns `SessionId` cookie (HTTP-only, Secure, SameSite=Strict)
   - JWT tokens stored server-side, never exposed to client

2. **Authenticated Requests**: 
   - Browser automatically includes `SessionId` cookie
   - Backend middleware retrieves JWT from session
   - No manual token management needed

3. **Token Refresh**:
   - Axios interceptor detects 401 responses
   - Automatically calls `/identity/refresh-token`
   - Updates session server-side, same `SessionId` cookie

4. **Logout**:
   - Calls `/identity/logout`
   - Server deletes session, clears cookie

### API Client Configuration

The `apiClient` in `src/api/client.ts` is pre-configured with:
- `withCredentials: true` for cookie handling
- Automatic token refresh on 401 errors
- Base URL from environment variable

**Always use `apiClient` for API calls** (not raw axios):
```typescript
import { apiClient } from '../api/client';

const response = await apiClient.post('/identity/sign-in', credentials);
```

## Routing & Authorization

### Route Protection

**PrivateRoute**: Ensures user is authenticated
```typescript
<PrivateRoute>
  <Dashboard />
</PrivateRoute>
```

**RoleRoute**: Restricts access by role
```typescript
<RoleRoute allowedRoleIds={[1]}>  {/* System Admin only */}
  <SystemSettings />
</RoleRoute>
```

### Role IDs
- `1` - System Admin
- `2` - Tenant Admin

See `src/routes/index.tsx` for complete route configuration.

## State Management

Redux Toolkit slices manage application state:

- **authSlice**: Current user, authentication status
- **usersSlice**: User list, selected user
- **rolesSlice**: Available roles
- **tenantsSlice**: Tenant data

**Use typed hooks** from `src/store/hooks.ts`:
```typescript
import { useAppDispatch, useAppSelector } from '../store/hooks';

const dispatch = useAppDispatch();
const currentUser = useAppSelector(state => state.auth.user);
```

## Type Safety

### API Types

Types are auto-generated from backend Swagger spec:

```typescript
import type { SigninRequest, UserOutput } from '../types';

const loginData: SigninRequest = {
  email: 'user@example.com',
  password: 'password',
};

const response = await apiClient.post<UserOutput>('/identity/sign-in', loginData);
```

**Regenerate types** after backend API changes:
```bash
npm run generate-types
```

## Styling

The app uses **Material-UI (MUI)** for components and theming:

- Theme configuration in `src/App.tsx`
- Custom components in `src/components/`
- MUI icons from `@mui/icons-material`

## Development Proxy

Vite dev server proxies API requests to avoid CORS issues:

```typescript
// vite.config.ts
server: {
  proxy: {
    '/api': { target: 'https://localhost:5001' },
    '/identity': { target: 'https://localhost:5001' },
  }
}
```

## Building for Production

```bash
npm run build
```

Production build output in `dist/` directory.

**Preview production build**:
```bash
npm run preview
```

## Code Conventions

- **Components**: PascalCase (e.g., `UserList.tsx`)
- **Hooks**: camelCase with `use` prefix (e.g., `useAuth.ts`)
- **API files**: kebab-case with `.api.ts` suffix (e.g., `users.api.ts`)
- **Types**: Import from `types/index.ts` barrel export
- **File organization**: Feature-based structure under `features/`

## Error Handling

Global error handling via Axios interceptors in `client.ts`:

- Network errors
- 401 (automatic token refresh)
- 403 (redirect to unauthorized page)
- 500 (server errors)

Component-level error handling with try-catch and user feedback via MUI Snackbar.

## Best Practices

1. **Always use typed hooks**: `useAppDispatch`, `useAppSelector`
2. **Import API client**: Use `apiClient` not raw axios
3. **Regenerate types**: Run `generate-types` after backend changes
4. **Protected routes**: Wrap with `PrivateRoute` or `RoleRoute`
5. **Type imports**: Import types from `types/index.ts`
6. **Environment variables**: Prefix with `VITE_` and access via `import.meta.env`

## Troubleshooting

### API Connection Issues
- Verify backend is running (`dotnet run` in `/server/AP.Platform`)
- Check `VITE_API_URL` in `.env` matches backend URL
- Ensure CORS is configured on backend

### Type Errors
- Regenerate types: `npm run generate-types`
- Check backend Swagger JSON is up to date

### Authentication Issues
- Check browser cookies (SessionId should be present)
- Verify backend session authentication is enabled
- Check browser console for 401/403 errors

## Further Reading

- [Vite Documentation](https://vitejs.dev/)
- [React 19 Documentation](https://react.dev/)
- [Redux Toolkit](https://redux-toolkit.js.org/)
- [Material-UI](https://mui.com/)
- [React Router v7](https://reactrouter.com/)
