import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { TEST_USER } from '../fixtures/test-user';
import { uniqueLabel } from '../fixtures/unique-label';

test.describe('Tree editing', () => {
  test.beforeEach(async ({ page }) => {
    const login = new LoginPage(page);
    const home = new HomePage(page);
    await login.open();
    await login.login(TEST_USER.email, TEST_USER.password);
    await home.waitForLoaded();
  });

  test('create a root section, render it as a heading, then delete it', async ({ page }) => {
    const home = new HomePage(page);
    const title = uniqueLabel('Section');

    await home.addRootSection(title);

    const line = home.lineByText(`# ${title}`).first();
    await expect(line).toBeVisible();

    const sectionId = await home.sectionIdForTitle(title);
    await home.deleteSection(sectionId);

    await expect(home.lineByText(`# ${title}`)).toHaveCount(0);
  });

  test('add a fact under a section, edit it, then delete it', async ({ page }) => {
    const home = new HomePage(page);
    const sectionTitle = uniqueLabel('Topic');
    const originalText = uniqueLabel('Fact saying something');
    const editedText = `${originalText} edited`;

    await home.addRootSection(sectionTitle);
    const sectionId = await home.sectionIdForTitle(sectionTitle);

    await home.addFactToSection(sectionId, originalText);
    await expect(home.lineByText(`- ${originalText}`)).toBeVisible();

    const factId = await home.factIdForText(originalText);
    await home.editFact(factId, editedText);
    await expect(home.lineByText(`- ${editedText}`)).toBeVisible();
    await expect(home.lineByText(`- ${originalText}`)).toHaveCount(0);

    await home.deleteFact(factId);
    await expect(home.lineByText(`- ${editedText}`)).toHaveCount(0);

    // Cleanup section
    await home.deleteSection(sectionId);
  });

  test('rename a section in place', async ({ page }) => {
    const home = new HomePage(page);
    const original = uniqueLabel('Before');
    const renamed = uniqueLabel('After');

    await home.addRootSection(original);
    const sectionId = await home.sectionIdForTitle(original);

    await home.renameSection(sectionId, renamed);
    await expect(home.lineByText(`# ${renamed}`)).toBeVisible();
    await expect(home.lineByText(`# ${original}`)).toHaveCount(0);

    await home.deleteSection(sectionId);
  });

  test('add a child section nested under a parent', async ({ page }) => {
    const home = new HomePage(page);
    const parent = uniqueLabel('Parent');
    const child = uniqueLabel('Child');

    await home.addRootSection(parent);
    const parentId = await home.sectionIdForTitle(parent);

    await home.addChildSection(parentId, child);
    await expect(home.lineByText(`## ${child}`)).toBeVisible();

    // Deleting the parent cascades and removes the child too.
    await home.deleteSection(parentId);
    await expect(home.lineByText(`# ${parent}`)).toHaveCount(0);
    await expect(home.lineByText(`## ${child}`)).toHaveCount(0);
  });

  test('cancelling the add-section prompt leaves the tree unchanged', async ({ page }) => {
    const home = new HomePage(page);

    const beforeCount = await home.allSectionLines().count();

    await home.addSectionFab.click();
    await page.locator('.cdk-overlay-container button:has-text("Cancel")').click();
    await expect(page.locator('.cdk-overlay-container .bd-dialog__title')).toHaveCount(0);

    const afterCount = await home.allSectionLines().count();
    expect(afterCount).toBe(beforeCount);
  });
});
