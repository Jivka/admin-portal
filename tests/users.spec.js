// @ts-check
import { test, expect } from '@playwright/test';

// ── Credentials ───────────────────────────────────────────────────────────────
const E2E_EMAIL    = process.env.E2E_EMAIL    ?? '';
const E2E_PASSWORD = process.env.E2E_PASSWORD ?? '';

// ── Unique data helpers ───────────────────────────────────────────────────────
/**
 * Generates a unique set of user fields safe for create/edit/search flows.
 * Using a timestamp + random suffix ensures no collision across runs.
 * @param {string} [prefix='E2E']
 */
function uniqueUserData(prefix = 'E2E') {
  const stamp = new Date().toISOString().replace(/[:.]/g, '-');
  const rand  = Math.random().toString(16).slice(2, 8);
  return {
    firstName: prefix,
    lastName:  `User-${stamp}-${rand}`,
    email:     `e2e-${stamp.toLowerCase()}-${rand}@test.example`,
  };
}

// ── Auth helper ───────────────────────────────────────────────────────────────
/**
 * Authenticates via the UI login form.
 * Session-based auth means we MUST go through the UI — no cookie injection.
 * @param {import('@playwright/test').Page} page
 */
async function login(page) {
  if (!E2E_EMAIL || !E2E_PASSWORD) {
    throw new Error('Set E2E_EMAIL and E2E_PASSWORD env vars for a System Admin test user.');
  }
  await page.goto('/login', { waitUntil: 'domcontentloaded' });
  if (/\/dashboard/i.test(page.url())) return;
  await page.getByRole('textbox', { name: /email/i }).fill(E2E_EMAIL);
  await page.locator('input[type="password"]').fill(E2E_PASSWORD);
  await page.getByRole('button', { name: /^sign in$/i }).click();
  await expect(page).toHaveURL(/\/dashboard/i, { timeout: 20_000 });
}

// ── Navigation helper ─────────────────────────────────────────────────────────
/**
 * Navigates to /users (requires System Admin or Tenant Admin role).
 * @param {import('@playwright/test').Page} page
 */
async function gotoUsers(page) {
  await page.goto('/users', { waitUntil: 'domcontentloaded' });
  if (/\/login/i.test(page.url())) {
    await login(page);
    await page.goto('/users', { waitUntil: 'domcontentloaded' });
  }
  const currentUrl = page.url();
  if (/\/dashboard/i.test(currentUrl)) {
    throw new Error(
      `Navigation to /users was blocked (likely missing role 1=System Admin or 2=Tenant Admin). Current URL: ${currentUrl}`
    );
  }
  if (/\/login/i.test(currentUrl)) {
    throw new Error(
      `Navigation to /users bounced back to /login (not authenticated). Ensure the app runs on HTTPS because SessionId cookie is Secure=true. Current URL: ${currentUrl}`
    );
  }
  await expect(page).toHaveURL(/\/users/i, { timeout: 20_000 });
  await expect(page.getByRole('heading', { name: /users management/i })).toBeVisible({ timeout: 20_000 });
}

// ── Create helper ─────────────────────────────────────────────────────────────
/**
 * Creates a user via the Add User dialog (page must already be on /users).
 * Populates state.firstName, state.lastName, state.email, state.fullName.
 * @param {import('@playwright/test').Page} page
 * @param {{ firstName: string, lastName: string, email: string, fullName: string, updatedLastName: string }} state
 */
async function createUserViaUI(page, state) {
  const data = uniqueUserData();
  state.firstName = data.firstName;
  state.lastName  = data.lastName;
  state.email     = data.email;
  state.fullName  = `${data.firstName} ${data.lastName}`;

  await page.getByRole('button', { name: /add user/i }).click();
  await expect(page.getByRole('heading', { name: /add new user/i })).toBeVisible();

  const dialog = page.getByRole('dialog');
  await dialog.getByLabel('First Name').fill(state.firstName);
  await dialog.getByLabel('Last Name').fill(state.lastName);
  await dialog.getByLabel('Email').fill(state.email);

  // Tenant autocomplete — select first available option
  await dialog.getByLabel('Tenant').click();
  const firstTenantOption = page.getByRole('option').first();
  await firstTenantOption.waitFor({ timeout: 10_000 });
  await firstTenantOption.click();

  // Role autocomplete — select "Test User" role
  await dialog.getByLabel('Role').click();
  const testUserRoleOption = page.getByRole('option', { name: /test user/i });
  await testUserRoleOption.waitFor({ timeout: 10_000 });
  await testUserRoleOption.click();

  // Verify the form is valid before submitting — if this fails, the autocomplete
  // selection did not propagate the tenantId/roleId into formData correctly.
  const submitBtn = dialog.getByRole('button', { name: /create user/i });
  await expect(submitBtn).toBeEnabled({ timeout: 10_000 });
  await submitBtn.click();
  // Assert via raw body text content to avoid MUI Snackbar portal / aria-hidden /
  // role-resolution issues that affect both getByRole() and CSS attribute selectors.
  await expect(page.locator('body')).toContainText(/user created successfully/i, { timeout: 20_000 });
}

// ── Test suite ────────────────────────────────────────────────────────────────
test.describe('Users (E2E)', () => {
  // Serial mode — tests share the user record created in the create test
  test.describe.configure({ mode: 'serial' });

  /** Shared mutable state across serial tests */
  const state = {
    firstName:       '',
    lastName:        '',
    email:           '',
    fullName:        '',
    updatedLastName: '',
  };

  // ── 1. Auth guard ────────────────────────────────────────────────────────
  test('redirects to login when unauthenticated', async ({ page }) => {
    await page.goto('/users', { waitUntil: 'domcontentloaded' });
    await expect(page).toHaveURL(/\/login/i);
    await expect(page.getByRole('heading', { name: /^login$/i })).toBeVisible();
  });

  // ── 2. List page loads ───────────────────────────────────────────────────
  test('loads the users list', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    // Main table with all expected column headers
    await expect(page.getByRole('table', { name: /users list/i })).toBeVisible({ timeout: 20_000 });
    await expect(page.getByRole('columnheader', { name: /^name$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: /^email$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: /^active$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: /^created on$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: /^actions$/i })).toBeVisible();

    // Pagination controls are rendered (MUI TablePagination always shows "Rows per page")
    await expect(page.getByText(/rows per page/i)).toBeVisible();
  });

  // ── 3. Create ────────────────────────────────────────────────────────────
  test('can create a user', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    await createUserViaUI(page, state);

    // Newly created user must appear in the list
    await expect(
      page.getByRole('row').filter({ hasText: state.fullName }).first()
    ).toBeVisible({ timeout: 20_000 });
  });

  // ── 4. Validation ────────────────────────────────────────────────────────
  test('shows Create User button as disabled when required fields are empty', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    await page.getByRole('button', { name: /add user/i }).click();
    await expect(page.getByRole('heading', { name: /add new user/i })).toBeVisible();

    const dialog = page.getByRole('dialog');
    // Button must be disabled until all required fields (firstName, lastName, email, tenant, role) are filled
    await expect(dialog.getByRole('button', { name: /create user/i })).toBeDisabled();

    // Dialog must remain open (no premature close on submit attempt)
    await expect(dialog).toBeVisible();

    await dialog.getByRole('button', { name: /cancel/i }).click();
    await expect(dialog).toBeHidden();
  });

  // ── 5. Edit ──────────────────────────────────────────────────────────────
  test('can edit a user', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    // Independent fallback — create if the create test was skipped
    if (!state.fullName) {
      await createUserViaUI(page, state);
    }

    state.updatedLastName = `${state.lastName}-Upd`;
    const updatedFullName = `${state.firstName} ${state.updatedLastName}`;

    const row = page.getByRole('row').filter({ hasText: state.fullName }).first();
    await expect(row).toBeVisible({ timeout: 20_000 });
    await row.getByRole('button', { name: /^edit user/i }).click();

    await expect(page.getByRole('heading', { name: /edit user/i })).toBeVisible();
    const editDialog = page.getByRole('dialog');
    const lastNameInput = editDialog.getByLabel('Last Name');
    await lastNameInput.clear();
    await lastNameInput.fill(state.updatedLastName);
    await editDialog.getByRole('button', { name: /update user/i }).click();

    await expect(page.locator('body')).toContainText(/user updated successfully/i, { timeout: 20_000 });

    // Update shared state so subsequent tests use the correct name
    state.fullName = updatedFullName;
    await expect(
      page.getByRole('row').filter({ hasText: state.fullName }).first()
    ).toBeVisible({ timeout: 20_000 });
  });

  // ── 6. Search / filter ───────────────────────────────────────────────────
  test('can search users by name', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    // Independent fallback
    if (!state.fullName) {
      await createUserViaUI(page, state);
    }

    // Use the unique part of the lastName so the search only hits our test record
    const searchTerm = state.updatedLastName || state.lastName;

    await page.getByLabel('Search by Name').fill(searchTerm);
    const matchingRow = page.getByRole('row').filter({ hasText: state.fullName }).first();
    await expect(matchingRow).toBeVisible({ timeout: 20_000 });

    // Clear search — our record must still be in the full list
    await page.getByLabel('Search by Name').fill('');
    await expect(
      page.getByRole('row').filter({ hasText: state.fullName }).first()
    ).toBeVisible({ timeout: 20_000 });
  });

  // ── 7. Delete ────────────────────────────────────────────────────────────
  test('can delete a user', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    // Independent fallback
    if (!state.fullName) {
      await createUserViaUI(page, state);
    }

    const row = page.getByRole('row').filter({ hasText: state.fullName }).first();
    await expect(row).toBeVisible({ timeout: 20_000 });
    await row.getByRole('button', { name: /^delete user/i }).click();

    const deleteDialog = page.getByRole('dialog');
    await expect(deleteDialog.getByRole('heading', { name: /confirm delete user/i })).toBeVisible();
    await deleteDialog.getByRole('button', { name: /^delete user$/i }).click();

    // Snackbar for delete success also uses role="status"
    await expect(page.locator('body')).toContainText(/deleted successfully/i, { timeout: 20_000 });
    await expect(
      page.getByRole('row').filter({ hasText: state.fullName }).first()
    ).toBeHidden({ timeout: 20_000 });
  });

  // ── 8. API error path ────────────────────────────────────────────────────
  test('shows inline error when create API call fails', async ({ page }) => {
    await login(page);
    await gotoUsers(page);

    // Intercept POST /api/users and force a 500 — also block refresh-token to
    // prevent infinite retry loop if a 401 were involved.
    await page.route('**/api/users', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ succeeded: false, errors: ['Internal server error'] }),
        });
      } else {
        await route.continue();
      }
    });

    await page.getByRole('button', { name: /add user/i }).click();
    await expect(page.getByRole('heading', { name: /add new user/i })).toBeVisible();

    const dialog = page.getByRole('dialog');
    await dialog.getByLabel('First Name').fill('Test');
    await dialog.getByLabel('Last Name').fill('ErrorCase');
    await dialog.getByLabel('Email').fill('error-test@test.example');

    // Select first available Tenant
    await dialog.getByLabel('Tenant').click();
    await page.getByRole('option').first().waitFor({ timeout: 10_000 });
    await page.getByRole('option').first().click();

    // Select first available Role
    await dialog.getByLabel('Role').click();
    await page.getByRole('option').first().waitFor({ timeout: 10_000 });
    await page.getByRole('option').first().click();

    await dialog.getByRole('button', { name: /create user/i }).click();

    // Error is rendered inline inside the dialog as an Alert (role="alert")
    await expect(dialog.getByRole('alert')).toBeVisible({ timeout: 20_000 });

    // Dialog must remain open — the error did not cause a premature close
    await expect(dialog).toBeVisible();
  });

  // ── Cleanup ──────────────────────────────────────────────────────────────
  /**
   * Teardown: delete the test user if it was not removed by the delete test.
   * This is a no-op when state.fullName is empty (create never ran or failed).
   */
  test.afterAll(async ({ browser }) => {
    const nameToDelete = state.fullName;
    if (!nameToDelete) return; // nothing was created — skip cleanup

    const context = await browser.newContext();
    const page    = await context.newPage();

    try {
      await login(page);
      await gotoUsers(page);

      const row = page.getByRole('row').filter({ hasText: nameToDelete }).first();
      if (!(await row.isVisible())) return; // delete test already removed it

      await row.getByRole('button', { name: /^delete user/i }).click();
      const dialog = page.getByRole('dialog');
      await dialog.getByRole('button', { name: /^delete user$/i }).click();
      await expect(page.locator('body')).toContainText(/deleted successfully/i, { timeout: 20_000 });
    } finally {
      await context.close();
    }
  });
});
