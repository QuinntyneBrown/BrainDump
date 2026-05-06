// Acceptance Test
// Traces to: L1-017, L2-039, L2-040
// Description: end-to-end coverage for the tab bar and split editor
// introduced by Slice 03.

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { PageTreePage } from '../pages/page-tree.page';
import { TabBarPage } from '../pages/tab-bar.page';
import { TEST_USER } from '../fixtures/test-user';
import { uniqueLabel } from '../fixtures/unique-label';

test.describe('Multi-document editing (Slice 03)', () => {
  test.beforeEach(async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);
    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();
  });

  test('opens a second document into a new tab', async ({ page }) => {
    const tree = new PageTreePage(page);
    const tabs = new TabBarPage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);

    // Both tabs are present in the left pane; the most recent is active.
    await tabs.expectTabCount('left', 2);
    await tabs.expectTabActive('left', docB);

    await tabs.clickTab('left', docA);
    await tabs.expectTabActive('left', docA);
  });

  test('closing the active tab activates a remaining tab', async ({ page }) => {
    const tree = new PageTreePage(page);
    const tabs = new TabBarPage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);

    await tabs.closeTab('left', docB);
    await tabs.expectTabCount('left', 1);
    await tabs.expectTabActive('left', docA);
  });

  test('split view shows two panes; clicking Split again collapses to one', async ({ page }) => {
    const tree = new PageTreePage(page);
    const tabs = new TabBarPage(page);

    const docA = uniqueLabel('A');
    await tree.createDocument(docA);

    await tabs.toggleSplit();
    await tabs.expectSplitActive();
    await tabs.expectTabActive('left', docA);
    await tabs.expectTabActive('right', docA);

    await tabs.toggleSplit();
    await tabs.expectNotSplit();
  });

  test('tab state persists across reload', async ({ page }) => {
    const tree = new PageTreePage(page);
    const tabs = new TabBarPage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);
    await tabs.clickTab('left', docA);

    // Allow the 500ms debounced PUT /api/tabs to fire.
    await page.waitForTimeout(800);

    await page.reload();
    const home = new HomePage(page);
    await home.waitForLoaded();

    await tabs.expectTabCount('left', 2);
    await tabs.expectTabActive('left', docA);
  });
});
