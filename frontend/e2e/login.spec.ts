import { test, expect } from '@playwright/test';

test('login end-to-end with PKCE', async ({ page }) => {
  // Unauthenticated visit redirects to /login
  await page.goto('/');
  await page.waitForURL('**/login');

  // Fill credentials using the Material text-field inputs
  await page.locator('input[type="email"]').fill('user@braindump.dev');
  await page.locator('input[type="password"]').fill('Password1!');

  // Submit the form
  await page.locator('button[type="submit"]').click();

  // Should land on the home page after successful PKCE login
  await page.waitForURL('**/home', { timeout: 10_000 });
  await expect(page.locator('h1')).toContainText('BrainDump');
});
