// @ts-check
import { test, expect } from '@playwright/test';

const E2E_EMAIL = process.env.E2E_EMAIL ?? '';
const E2E_PASSWORD = process.env.E2E_PASSWORD ?? '';

/**
 * Generates a unique tenant name safe for display/search.
 */
function uniqueTenantName(prefix = 'E2E Tenant') {
  const stamp = new Date().toISOString().replace(/[:.]/g, '-');
  const rand = Math.random().toString(16).slice(2, 8);
  return `${prefix} ${stamp} ${rand}`;
}

/**
 * Generates a unique BIC so repeated runs don't hit the unique-BIC constraint.
 */
function uniqueBIC() {
  return `E2E${Date.now().toString(36).toUpperCase().slice(-6)}`;
}

/**
 * Creates a tenant via the Add Tenant dialog.
 * Requires the page to already be on /tenants.
 * Populates state.tenantName and state.tenantBIC.
 * @param {import('@playwright/test').Page} page
 * @param {{ tenantName: string, tenantBIC: string }} state
 */
async function createTenantViaUI(page, state) {
  state.tenantName = uniqueTenantName();
  state.tenantBIC = uniqueBIC();

  await page.getByRole('button', { name: /add tenant/i }).click();
  await expect(page.getByRole('heading', { name: /add new tenant/i })).toBeVisible();

  const dialog = page.getByRole('dialog');
  await dialog.getByLabel('Tenant Name').fill(state.tenantName);
  await dialog.getByLabel('Tenant BIC').fill(state.tenantBIC);

  await page.getByRole('button', { name: /create tenant/i }).click();
  await expect(page.getByRole('alert')).toContainText(/tenant created successfully/i, { timeout: 20_000 });
}

/**
 * Ensures we are authenticated by logging in through the UI.
 * This project uses session cookies (SessionId), so UI login is the most reliable.
 * @param {import('@playwright/test').Page} page
 */
async function login(page) {
  if (!E2E_EMAIL || !E2E_PASSWORD) {
    throw new Error('Set E2E_EMAIL and E2E_PASSWORD env vars for a System Admin test user.');
  }

  await page.goto('/login', { waitUntil: 'domcontentloaded' });

  // If we are already logged in, the app may redirect away from /login.
  if (/\/dashboard/i.test(page.url())) return;

  await page.getByRole('textbox', { name: /email/i }).fill(E2E_EMAIL);
  await page.locator('input[type="password"]').fill(E2E_PASSWORD);
  await page.getByRole('button', { name: /^sign in$/i }).click();

  await expect(page).toHaveURL(/\/dashboard/i, { timeout: 20_000 });
}

/**
 * Navigates to Tenants page (requires System Admin or Tenant Admin).
 * @param {import('@playwright/test').Page} page
 */
async function gotoTenants(page) {
  await page.goto('/tenants', { waitUntil: 'domcontentloaded' });

  // PrivateRoute will redirect unauthenticated users to /login.
  if (/\/login/i.test(page.url())) {
    await login(page);
    await page.goto('/tenants', { waitUntil: 'domcontentloaded' });
  }

  // RoleRoute may redirect unauthorized users to /dashboard.
  const currentUrl = page.url();
  if (/\/dashboard/i.test(currentUrl)) {
    throw new Error(
      `Navigation to /tenants was blocked (likely missing role 1=System Admin or 2=Tenant Admin). Current URL: ${currentUrl}`
    );
  }
  if (/\/login/i.test(currentUrl)) {
    throw new Error(
      `Navigation to /tenants bounced back to /login (not authenticated). Ensure the app runs on HTTPS because SessionId cookie is Secure=true. Current URL: ${currentUrl}`
    );
  }

  await expect(page).toHaveURL(/\/tenants/i, { timeout: 20_000 });

  const tenantsHeading = page.getByRole('heading', { name: /tenants management/i });
  try {
    await expect(tenantsHeading).toBeVisible({ timeout: 20_000 });
  } catch {
    await expect(page.getByText('Tenants Management')).toBeVisible({ timeout: 20_000 });
  }
}

test.describe('Tenants (E2E)', () => {
  test.describe.configure({ mode: 'serial' });

  const state = {
    tenantName: '',
    tenantBIC: '',
    updatedTenantName: '',
  };

  test('redirects to login when unauthenticated', async ({ page }) => {
    await page.goto('/tenants', { waitUntil: 'domcontentloaded' });
    await expect(page).toHaveURL(/\/login/i);
    await expect(page.getByRole('heading', { name: /^login$/i })).toBeVisible();
  });

  test('can create a tenant', async ({ page }) => {
    await login(page);
    await gotoTenants(page);

    await createTenantViaUI(page, state);

    await expect(page.getByText(state.tenantName)).toBeVisible({ timeout: 20_000 });
  });

  test('can edit a tenant', async ({ page }) => {
    await login(page);
    await gotoTenants(page);

    // If the create test didn't run or failed, create a tenant now so this
    // test can run independently.
    if (!state.tenantName) {
      await createTenantViaUI(page, state);
    }

    state.updatedTenantName = `${state.tenantName} (Updated)`;

    const row = page.getByRole('row').filter({ hasText: state.tenantName }).first();
    await expect(row).toBeVisible({ timeout: 20_000 });

    await row.getByRole('button', { name: /^edit$/i }).click();
    await expect(page.getByRole('heading', { name: /edit tenant/i })).toBeVisible();

    const editDialog = page.getByRole('dialog');
    await editDialog.getByLabel('Tenant Name').fill(state.updatedTenantName);
    await page.getByRole('button', { name: /update tenant/i }).click();

    await expect(page.getByRole('alert')).toContainText(/tenant updated successfully/i, { timeout: 20_000 });
    await expect(page.getByText(state.updatedTenantName)).toBeVisible({ timeout: 20_000 });
  });

  test('can delete a tenant', async ({ page }) => {
    await login(page);
    await gotoTenants(page);

    // If prior tests didn't run or failed, create a fresh tenant to delete.
    if (!state.updatedTenantName && !state.tenantName) {
      await createTenantViaUI(page, state);
    }

    const nameToDelete = state.updatedTenantName || state.tenantName;

    const row = page.getByRole('row').filter({ hasText: nameToDelete }).first();
    await expect(row).toBeVisible({ timeout: 20_000 });

    await row.getByRole('button', { name: /^delete$/i }).click();
    const deleteDialog = page.getByRole('dialog');
    await expect(deleteDialog.getByRole('heading', { name: /confirm delete/i }).first()).toBeVisible();

    await deleteDialog.getByRole('button', { name: /^delete$/i }).click();

    await expect(page.getByRole('alert')).toContainText(/deleted successfully/i, { timeout: 20_000 });
    await expect(page.getByText(nameToDelete)).toBeHidden({ timeout: 20_000 });
  });
});
