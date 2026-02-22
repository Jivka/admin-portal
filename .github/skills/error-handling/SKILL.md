---
name: error-handling
description: Common, backend-driven error handling patterns for React API calls (Axios + ApiResult).
---

This skill defines how the React frontend should handle and display errors from backend API calls.

Core rule: **show backend validation messages as-is when provided**. Do not invent or “improve” backend validation text.

## Use the shared API client

- Always call backend endpoints via `apiClient` (Axios instance) from `client/src/api/client.ts`.
- `apiClient` is configured with `withCredentials: true` (session cookie) and auto refreshes tokens on `401` for non-auth endpoints.

## Backend error formats to support

The .NET backend returns errors via `ApiResult`/`ApiError`.

### Single error object

Typical JSON shape (note the hyphenated keys):

```json
{
	"error-code": "SOME_CODE",
	"error-message": "Human readable message",
	"error-details": ["Optional", "additional", "details"]
}
```

Display rules:

- If `error-details` exists and has items, show them (one line if single item; list if multiple).
- Otherwise show `error-message`.

### Multiple errors (array)

Some endpoints may return a JSON array of `ApiError` objects:

```json
[
	{ "error-code": "A", "error-message": "Message A" },
	{ "error-code": "B", "error-message": "Message B" }
]
```

Display rules:

- Show the messages as a list.
- Do not merge or rewrite the messages.

## UI patterns (React + MUI)

### Preferred: store the `AxiosError` and render a single error component

- Keep error state typed as `AxiosError | null`.
- Pass it into the shared `ErrorAlert` component (`client/src/components/common/ErrorAlert.tsx`) to render backend messages.

Example:

```tsx
import type { AxiosError } from 'axios';
import { ErrorAlert } from '../../components/common/ErrorAlert';

const [error, setError] = useState<AxiosError | null>(null);

try {
	setError(null);
	await usersApi.createUser(payload);
} catch (err) {
	setError(err as AxiosError);
}

return <ErrorAlert error={error} />;
```

Why: it ensures backend-driven messages are displayed consistently and avoids `"[object Object]"` issues.

### Snackbar/toast messages

- Success snackbars can use frontend text.
- Error snackbars should prefer backend messages.
- Avoid `message: `Failed ...: ${err}` because it stringifies objects poorly.

If you need a string for a snackbar:

- Extract it from `AxiosError.response.data` using the same rules as above (`error-details` → `error-message` → array messages).
- If there is truly no backend payload (network error / CORS / request cancelled), it’s acceptable to show a generic connectivity message.

### Forms and field-level errors

- Only show field-specific helper text if the backend provides field-specific information.
- If the backend returns only a general message / list, display it near the top of the form (e.g., via `ErrorAlert`) rather than guessing which field is wrong.

## Status-code behavior

- `401 Unauthorized`: handled centrally by `apiClient` refresh logic for most endpoints; if refresh fails the app redirects to `/login`.
- `403 Forbidden`: display the backend message (don’t replace it with a custom “no access” sentence).
- `400 Bad Request` validation: display backend validation messages (often in `error-details` or `error-message`).
- `5xx`: display backend message if present; otherwise show a generic fallback.

## What not to do

- Don’t replace backend validation with custom wording.
- Don’t assume a validation schema that the backend didn’t return.
- Don’t swallow errors in API wrappers (let the caller decide how to render them).
- Don’t mix string and `AxiosError` types in the same `error` state; pick one pattern per component.

## Quick checklist for new API calls

- Use `apiClient`.
- Catch errors at the UI boundary (page/dialog/form).
- Store `AxiosError` and render `ErrorAlert`.
- For snackbars, prefer backend-derived message.
