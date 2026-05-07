// Acceptance Test
// Traces to: L1-019, L2-043, L2-044
// Description: end-to-end coverage for [[wiki-link]] extraction and the
// right-rail backlinks panel introduced by Slice 06.

import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { PageTreePage } from '../pages/page-tree.page';
import { BacklinksPage } from '../pages/backlinks.page';
import { TEST_USER } from '../fixtures/test-user';
import { uniqueLabel } from '../fixtures/unique-label';

test.describe('Cross-document backlinks (Slice 06)', () => {
  test.beforeEach(async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);
    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();
  });

  test('a fact with [[target-title]] surfaces on the target backlinks panel', async ({ page }) => {
    const tree = new PageTreePage(page);
    const home = new HomePage(page);
    const backlinks = new BacklinksPage(page);

    const targetTitle = uniqueLabel('target');
    const sourceTitle = uniqueLabel('source');
    await tree.createDocument(targetTitle);
    await tree.createDocument(sourceTitle);

    // Active doc is sourceTitle (most recently created). Add a fact that
    // links to the target.
    const sectionTitle = uniqueLabel('section');
    await home.addRootSection(sectionTitle);
    const sectionId = await home.sectionIdForTitle(sectionTitle);
    await home.addFactToSection(sectionId, `see [[${targetTitle}]]`);

    // Switch to the target and confirm the backlink shows up.
    await tree.openDocument(targetTitle);
    await backlinks.expectCard(sourceTitle);
  });

  test('removing the link from the source doc removes the backlink', async ({ page }) => {
    const tree = new PageTreePage(page);
    const home = new HomePage(page);
    const backlinks = new BacklinksPage(page);

    const targetTitle = uniqueLabel('target');
    const sourceTitle = uniqueLabel('source');
    await tree.createDocument(targetTitle);
    await tree.createDocument(sourceTitle);
    const sectionTitle = uniqueLabel('section');
    await home.addRootSection(sectionTitle);
    const sectionId = await home.sectionIdForTitle(sectionTitle);
    await home.addFactToSection(sectionId, `link [[${targetTitle}]]`);

    await tree.openDocument(targetTitle);
    await backlinks.expectCard(sourceTitle);

    // Edit the source's fact to drop the link.
    await tree.openDocument(sourceTitle);
    const factId = await home.factIdForText(`link [[${targetTitle}]]`);
    await home.editFact(factId, 'no link anymore');

    await tree.openDocument(targetTitle);
    await backlinks.expectNoCard(sourceTitle);
  });
});
