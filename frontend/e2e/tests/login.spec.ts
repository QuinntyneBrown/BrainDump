import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { TEST_USER } from '../fixtures/test-user';

test.describe('Login', () => {
  test('unauthenticated visit redirects to /login', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto('/');
    await page.waitForURL('**/login');
    await login.expectLoaded();
  });

  test('valid credentials complete the PKCE flow and land on /home', async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);

    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);

    await home.waitForLoaded();
    await expect(page).toHaveURL(/\/home$/);
  });

  test('invalid credentials surface an error and stay on /login', async ({ page }) => {
    const login = new LoginPage(page);

    await login.open();
    await login.login('wrong@example.com', 'NotMyPassword!');

    await login.expectError(/invalid/i);
    await expect(page).toHaveURL(/\/login$/);
  });

  test('signing out from /home redirects back to /login', async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);

    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();

    await home.signOut();
    await login.expectLoaded();
  });
});
