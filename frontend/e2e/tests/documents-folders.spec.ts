// Acceptance Test
// Traces to: L1-015, L1-016, L2-033..L2-038
// Description: happy path through Slice 02 — create folder, create document
// inside it, the document becomes active, the editor shows the empty-state
// for an unedited document.

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { PageTreePage } from '../pages/page-tree.page';
import { TEST_USER } from '../fixtures/test-user';
import { uniqueLabel } from '../fixtures/unique-label';

test.describe('Documents and Folders (Slice 02)', () => {
  test.beforeEach(async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);
    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();
  });

  test('create root folder, create document inside it, document becomes active', async ({ page }) => {
    const home = new HomePage(page);
    const tree = new PageTreePage(page);

    const folderTitle = uniqueLabel('Engineering');
    const docTitle = uniqueLabel('brain-dump.md');

    await tree.createFolder(folderTitle);
    await tree.createDocument(docTitle);

    await tree.expectDocumentVisible(docTitle);
    await tree.expectDocumentActive(docTitle);
    await expect(home.topBarTitle).toContainText(docTitle);
  });

  test('opening a different document switches the editor active doc', async ({ page }) => {
    const tree = new PageTreePage(page);
    const home = new HomePage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);

    // Most recently created becomes active.
    await tree.expectDocumentActive(docB);

    await tree.openDocument(docA);
    await tree.expectDocumentActive(docA);
    await expect(home.topBarTitle).toContainText(docA);
  });
});
