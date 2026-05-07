// Acceptance Test
// Traces to: L1-018, L2-041, L2-042
// Description: end-to-end coverage for the right-rail labels editor and
// the page-tree filter chips introduced by Slice 05.

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { PageTreePage } from '../pages/page-tree.page';
import { LabelsPage } from '../pages/labels.page';
import { TEST_USER } from '../fixtures/test-user';
import { uniqueLabel } from '../fixtures/unique-label';

test.describe('Document labels (Slice 05)', () => {
  test.beforeEach(async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);
    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();
  });

  test('add and remove a label on the active document', async ({ page }) => {
    const tree = new PageTreePage(page);
    const labels = new LabelsPage(page);

    await tree.createDocument(uniqueLabel('doc'));

    const labelName = uniqueLabel('engineering').toLowerCase();
    await labels.addLabel(labelName);
    await labels.expectChip(labelName);

    await labels.removeLabel(labelName);
    await expect(labels.chip(labelName)).toHaveCount(0);
  });

  test('a workspace label appears as a filter chip and narrows the page tree', async ({ page }) => {
    const tree = new PageTreePage(page);
    const labels = new LabelsPage(page);

    const docA = uniqueLabel('A');
    const docB = uniqueLabel('B');
    await tree.createDocument(docA);
    await tree.createDocument(docB);

    // Active doc is docB; tag it.
    const labelName = uniqueLabel('only-b').toLowerCase();
    await labels.addLabel(labelName);
    await labels.expectFilterChip(labelName);

    await labels.clickFilterChip(labelName);
    await expect(tree.documentRow(docB)).toBeVisible();
    await expect(tree.documentRow(docA)).toHaveCount(0);

    await labels.clickFilterChip('all');
    await expect(tree.documentRow(docA)).toBeVisible();
    await expect(tree.documentRow(docB)).toBeVisible();
  });
});
