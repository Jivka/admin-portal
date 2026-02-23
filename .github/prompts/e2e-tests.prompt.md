---
name: 'e2e-tests'
description: 'Generate Playwright E2E tests for a feature [FEATURE NAME]. Covers auth, CRUD flows, role guards, and error states.'
---

# Generate E2E Tests for a Feature

## Context

You are writing **Playwright E2E tests** (JavaScript, `@playwright/test`) for the **[FEATURE NAME]** feature located at `client/src/features/[feature-dir]/`.

The app is a **React 19 + TypeScript + MUI v5** SPA backed by a **.NET 9 API**. Authentication uses **session-based cookies** (`SessionId` HTTP-only cookie — not a JWT cookie). Login must always go through the UI.

Place the new test file at `tests/[feature-name].spec.js`.

---

## Pre-requisites

- The frontend dev server must be running (`npm run dev` in `client/`).
- The backend must be running (`dotnet run` in `server/AP.Platform/`).
- Environment variables `E2E_EMAIL` and `E2E_PASSWORD` must be set (a **System Admin** account), either in `.env.e2e` or the shell. Optionally define `E2E_TENANT_ADMIN_EMAIL` / `E2E_TENANT_ADMIN_PASSWORD` for role-restricted tests.
- `PLAYWRIGHT_BASE_URL` defaults to `http://localhost:5173` (set in `playwright.config.js`).

---

## File Structure Requirements

```js
// @ts-check
import { test, expect } from '@playwright/test';

// ── Credentials ──────────────────────────────────────────────────────────────
const E2E_EMAIL          = process.env.E2E_EMAIL          ?? '';
const E2E_PASSWORD       = process.env.E2E_PASSWORD       ?? '';
// Add role-specific credentials only if the feature has role restrictions:
// const E2E_TENANT_ADMIN_EMAIL    = process.env.E2E_TENANT_ADMIN_EMAIL    ?? '';
// const E2E_TENANT_ADMIN_PASSWORD = process.env.E2E_TENANT_ADMIN_PASSWORD ?? '';
```

---

## Required Helper Functions

Always include the following helpers (adapt as needed for the feature):

### `login(page)`
```js
async function login(page) {
  if (!E2E_EMAIL || !E2E_PASSWORD) {
    throw new Error('Set E2E_EMAIL and E2E_PASSWORD env vars.');
  }
  await page.goto('/login', { waitUntil: 'domcontentloaded' });
  if (/\/dashboard/i.test(page.url())) return; // already authenticated
  await page.getByRole('textbox', { name: /email/i }).fill(E2E_EMAIL);
  await page.locator('input[type="password"]').fill(E2E_PASSWORD);
  await page.getByRole('button', { name: /^sign in$/i }).click();
  await expect(page).toHaveURL(/\/dashboard/i, { timeout: 20_000 });
}
```

### `gotoFeature(page)` — replace `/[feature-route]` and heading text
```js
async function gotoFeature(page) {
  await page.goto('/[feature-route]', { waitUntil: 'domcontentloaded' });
  if (/\/login/i.test(page.url())) {
    await login(page);
    await page.goto('/[feature-route]', { waitUntil: 'domcontentloaded' });
  }
  const currentUrl = page.url();
  if (/\/dashboard/i.test(currentUrl)) {
    throw new Error('Navigation blocked — check role permissions.');
  }
  await expect(page).toHaveURL(/\/[feature-route]/i, { timeout: 20_000 });
  await expect(page.getByRole('heading', { name: /[Feature Heading]/i })).toBeVisible({ timeout: 20_000 });
}
```

### Unique data helpers (only for features with create/edit flows)
Generate unique names, codes, or identifiers to avoid collisions across runs:
```js
function uniqueName(prefix = 'E2E Item') {
  const stamp = new Date().toISOString().replace(/[:.]/g, '-');
  const rand  = Math.random().toString(16).slice(2, 8);
  return `${prefix} ${stamp} ${rand}`;
}
```

---

## Test Suite Structure

Use `test.describe('[Feature] (E2E)', ...)` with `test.describe.configure({ mode: 'serial' })` when tests share mutable state (e.g., a created record that later tests edit/delete). Use the default parallel mode for fully independent tests.

Maintain a `state` object at the `describe` scope to share data between serial tests:
```js
const state = { itemName: '', updatedItemName: '' };
```

---

## Required Test Cases

Implement **all of the following** that apply to the feature. Skip any that are not relevant (e.g., no delete flow → skip delete test) and add a comment explaining why.

### 1. Auth guard
```js
test('redirects to login when unauthenticated', async ({ page }) => {
  await page.goto('/[feature-route]', { waitUntil: 'domcontentloaded' });
  await expect(page).toHaveURL(/\/login/i);
  await expect(page.getByRole('heading', { name: /^login$/i })).toBeVisible();
});
```

### 2. Role guard (if the feature is role-restricted)
```js
test('redirects to dashboard for unauthorized role', async ({ page }) => {
  // Log in as a lower-privilege user (e.g., Tenant Admin trying to access System-Admin-only page)
  // ...
  await expect(page).toHaveURL(/\/dashboard/i, { timeout: 20_000 });
});
```

### 3. Read / list page loads
```js
test('loads the [feature] list', async ({ page }) => {
  await login(page);
  await gotoFeature(page);
  // Assert table/list is visible, check column headers, verify at least one row if seeded data exists
});
```

### 4. Create (if applicable)
- Click the "Add …" button.
- Assert the dialog/form opens with the correct heading.
- Fill in all required fields using unique test data.
- Submit and assert a **success alert** (`role="alert"`) with the expected message.
- Assert the new item appears in the list.

### 5. Validation (if applicable)
- Submit the create/edit form with missing or invalid data.
- Assert inline field errors are visible.
- Assert the dialog remains open (no premature close).

### 6. Edit (if applicable)
- Locate the created row by name.
- Click the Edit button.
- Modify at least one field.
- Submit and assert the success alert.
- Assert the updated value is visible in the list.

### 7. Delete (if applicable)
- Locate the target row.
- Click the Delete button.
- Assert a **confirmation dialog** opens.
- Confirm deletion.
- Assert the success alert.
- Assert the item is no longer visible in the list (`toBeHidden`).

### 8. Search / filter (if the feature has a search input)
- Type a known value into the search field.
- Assert only matching rows are shown.
- Clear the search and assert all rows return.

### 9. Pagination (if the feature has a paginated table)
- Assert the first page is loaded.
- Navigate to the next page and assert rows change.

---

## Cleanup / Teardown

Every test session **must leave the database in the same state it found it**. Any record created during a test run must be deleted before the suite finishes, even when a test fails mid-way.

### Rules

1. **Always delete what you create.** If a test creates a record, that record must be removed in an `afterAll` (for `serial` suites) or `afterEach` (for parallel suites) hook.
2. **Use the UI to clean up** — delete via the same Delete flow the user would use. This also exercises the delete path if it has not been covered by a dedicated test.
3. **Guard against missing state.** The cleanup hook must be a no-op if the record was never created (e.g., the create test failed before writing to `state`):
   ```js
   test.afterAll(async ({ page }) => {
     if (!state.itemName && !state.updatedItemName) return; // nothing to clean up
     await login(page);
     await gotoFeature(page);
     // Delete the item via UI …
   });
   ```
4. **Unique names make cleanup reliable.** Because test data uses unique generated names (see [Unique data helpers](#unique-data-helpers)), a targeted row filter will never accidentally match real data:
   ```js
   const row = page.getByRole('row').filter({ hasText: state.itemName }).first();
   ```
5. **API-intercept tests need no cleanup** — they mock the network response, so no real record is created in those tests.
6. **Parallel suites** — when `mode` is not `serial`, use `afterEach` to delete the record created within that specific test, and store the created name in a local variable rather than shared `state`.

### Serial suite teardown template

```js
test.afterAll(async ({ browser }) => {
  // afterAll receives no `page` fixture — create a fresh context instead.
  const nameToDelete = state.updatedItemName || state.itemName;
  if (!nameToDelete) return;

  const context = await browser.newContext();
  const page    = await context.newPage();

  try {
    await login(page);
    await gotoFeature(page);

    const row = page.getByRole('row').filter({ hasText: nameToDelete }).first();
    if (!(await row.isVisible())) return; // already gone (delete test ran successfully)

    await row.getByRole('button', { name: /^delete$/i }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByRole('button', { name: /^delete$/i }).click();
    await expect(page.getByRole('alert')).toContainText(/deleted successfully/i, { timeout: 20_000 });
  } finally {
    await context.close();
  }
});
```

---

## Assertions & Selectors — Rules

1. **Prefer semantic selectors** in this order:
   - `getByRole` with `name` option (most robust)
   - `getByLabel` for form inputs
   - `getByText` as a last resort
   - Never use `locator('.some-class')` or `locator('#some-id')` unless the element has no accessible name.
2. **Always scope dialog interactions** via `page.getByRole('dialog')` before querying child elements.
3. **Success/error messages** — always assert via `page.getByRole('alert')`. The MUI `<Alert>` component renders with `role="alert"`.
4. **Timeouts** — use `{ timeout: 20_000 }` for assertions that depend on API calls (create, update, delete, navigate). Use the default for purely synchronous DOM checks.
5. **Never hard-code test data** that may already exist in the database. Always use unique generators.
6. **Independent fallback** — each test that depends on a created record should check `state.itemName` and call the creation helper itself if the prior test was skipped:
   ```js
   if (!state.itemName) { await createItemViaUI(page, state); }
   ```

---

## Error State Tests

Include at least one test that exercises the **API error path** (network failure or validation rejection from the server):

```js
test('shows error when API call fails', async ({ page }) => {
  await login(page);
  await gotoFeature(page);

  // Intercept the relevant API call and force a 500 response
  await page.route('**/api/[endpoint]', route =>
    route.fulfill({ status: 500, body: JSON.stringify({ succeeded: false, errors: ['Internal server error'] }) })
  );

  // Trigger the action (e.g., open dialog and submit)
  // ...

  // Assert error feedback is visible
  await expect(page.getByRole('alert')).toContainText(/error|failed/i);
});
```

---

## Session Authentication Notes

- The app sets a `SessionId` **HTTP-only, Secure, SameSite=Strict** cookie. There is no way to inject auth state via `localStorage` or `document.cookie`.
- Always authenticate through the UI `login()` helper — never fabricate cookies or call the API directly in tests.
- If the backend runs on HTTPS with a self-signed cert, ensure `ignoreHTTPSErrors: true` is set in `playwright.config.js` (it already is by default in this project).
- The Axios client on the frontend automatically retries on 401 by calling `/identity/refresh-token`. If a test intentionally triggers a 401, intercept `**/identity/refresh-token` as well to prevent an infinite retry loop.

---

## Running the Tests

```powershell
# Run all tests for this feature (Chromium only, faster for local dev)
npm run test:e2e -- --project=chromium tests/[feature-name].spec.js

# Run a single test by title
npm run test:e2e -- --project=chromium tests/[feature-name].spec.js -g "can create"

# Show HTML report after a run
npx playwright show-report
```

---

## Checklist Before Committing

- [ ] No `test.only` calls left in the file.
- [ ] All selectors are semantic (`getByRole`, `getByLabel`).
- [ ] Every API-dependent assertion uses `{ timeout: 20_000 }`.
- [ ] Unique data generators are used for all created records.
- [ ] Auth guard test is present.
- [ ] Serial mode is enabled for tests that share mutable state.
- [ ] Error/API-failure path is covered by at least one test.
- [ ] `afterAll` / `afterEach` cleanup hook is present and deletes all records created during the run.
- [ ] Cleanup hook is a no-op (early return) when no data was created (guards against failed create tests).
