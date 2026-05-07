import { Locator, Page, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object for the right-rail labels editor and the page-tree filter
 * chips introduced by Slice 05 (docs/detailed-designs/05-document-labels).
 */
export class LabelsPage extends BasePage {
  readonly editor: Locator;
  readonly addButton: Locator;

  constructor(page: Page) {
    super(page);
    this.editor = page.locator('[data-testid="labels-editor"]');
    this.addButton = page.locator('[data-testid="add-label"]');
  }

  chip(label: string): Locator {
    return this.editor.locator(`.home-shell__label-chip[data-label="${label}"]`);
  }

  async addLabel(label: string): Promise<void> {
    await this.addButton.click();
    const input = this.page.locator('.cdk-overlay-container input');
    await input.waitFor({ state: 'visible' });
    await input.fill(label);
    await this.page.locator('.cdk-overlay-container button:has-text("Save")').click();
    await this.page.locator('.cdk-overlay-container .bd-dialog__title').waitFor({ state: 'detached' });
    await expect(this.chip(label)).toBeVisible();
  }

  async removeLabel(label: string): Promise<void> {
    await this.chip(label).locator('.home-shell__label-remove').click();
    await expect(this.chip(label)).toHaveCount(0);
  }

  async expectChip(label: string): Promise<void> {
    await expect(this.chip(label)).toBeVisible();
  }

  async expectFilterChip(label: string): Promise<void> {
    await expect(this.page.locator(`[data-testid="filter-${label}"]`)).toBeVisible();
  }

  async clickFilterChip(label: string | 'all'): Promise<void> {
    await this.page.locator(`[data-testid="filter-${label}"]`).click();
  }
}
