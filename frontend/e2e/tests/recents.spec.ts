// Acceptance Test
// Traces to: L1-021, L2-047
// Description: end-to-end coverage for the "Recently viewed" sidebar
// section introduced by Slice 04.

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { PageTreePage } from '../pages/page-tree.page';
import { RecentsPage } from '../pages/recents.page';
import { TabBarPage } from '../pages/tab-bar.page';
import { TEST_USER } from '../fixtures/test-user';
import { uniqueLabel } from '../fixtures/unique-label';

test.describe('Recently viewed (Slice 04)', () => {
  test.beforeEach(async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);
    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();
  });

  test('opening a document moves it to the top of recents', async ({ page }) => {
    const tree = new PageTreePage(page);
    const tabs = new TabBarPage(page);
    const recents = new RecentsPage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);

    // Most recently created becomes active, so docB views first; then opening docA bumps it to top.
    await tabs.clickTab('left', docA);
    await expect(recents.rowByTitle(docA)).toBeVisible();
    await expect(recents.rows().first()).toContainText(docA);
  });

  test('recents persists across reload', async ({ page }) => {
    const tree = new PageTreePage(page);
    const recents = new RecentsPage(page);

    const docA = uniqueLabel('A');
    await tree.createDocument(docA);
    await expect(recents.rowByTitle(docA)).toBeVisible();

    await page.reload();
    const home = new HomePage(page);
    await home.waitForLoaded();

    await expect(recents.rowByTitle(docA)).toBeVisible();
  });

  test('clicking a recent entry opens that document', async ({ page }) => {
    const tree = new PageTreePage(page);
    const tabs = new TabBarPage(page);
    const recents = new RecentsPage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);

    await recents.clickRow(docA);
    await tabs.expectTabActive('left', docA);
  });
});
