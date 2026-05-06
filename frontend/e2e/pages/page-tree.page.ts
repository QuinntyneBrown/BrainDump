import { Locator, Page, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object for the page-tree sidebar introduced by Slice 02
 * (docs/detailed-designs/02-documents-and-folders).
 */
export class PageTreePage extends BasePage {
  readonly tree: Locator;
  readonly addDocumentBtn: Locator;
  readonly addFolderBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.tree = page.locator('[data-testid="page-tree"]');
    this.addDocumentBtn = page.locator('[data-testid="add-document"] button');
    this.addFolderBtn = page.locator('[data-testid="add-folder"] button');
  }

  documentRow(title: string): Locator {
    return this.tree.locator('.bd-page-tree__row--document', { hasText: title });
  }

  folderRow(title: string): Locator {
    return this.tree.locator('.bd-page-tree__row--folder', { hasText: title });
  }

  async createDocument(title: string): Promise<void> {
    await this.addDocumentBtn.click();
    await this.fillPromptDialog(title);
    await expect(this.documentRow(title)).toBeVisible();
  }

  async createFolder(title: string): Promise<void> {
    await this.addFolderBtn.click();
    await this.fillPromptDialog(title);
    await expect(this.folderRow(title)).toBeVisible();
  }

  async openDocument(title: string): Promise<void> {
    await this.documentRow(title).click();
    await expect(this.documentRow(title)).toHaveAttribute('data-active', 'true');
  }

  async expectDocumentActive(title: string): Promise<void> {
    await expect(this.documentRow(title)).toHaveAttribute('data-active', 'true');
  }

  async expectDocumentVisible(title: string): Promise<void> {
    await expect(this.documentRow(title)).toBeVisible();
  }

  private async fillPromptDialog(text: string): Promise<void> {
    const input = this.page.locator('.cdk-overlay-container input');
    await input.waitFor({ state: 'visible' });
    await input.fill(text);
    await this.page.locator('.cdk-overlay-container button:has-text("Save")').click();
    await this.page.locator('.cdk-overlay-container .bd-dialog__title').waitFor({ state: 'detached' });
  }
}
