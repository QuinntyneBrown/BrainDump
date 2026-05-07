import { Locator, Page, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object for the right-rail backlinks panel introduced by Slice 06
 * (docs/detailed-designs/06-backlinks).
 */
export class BacklinksPage extends BasePage {
  readonly panel: Locator;

  constructor(page: Page) {
    super(page);
    this.panel = page.locator('[data-testid="backlinks"]');
  }

  cards(): Locator {
    return this.panel.locator('.bd-backlinks__card');
  }

  cardByTitle(title: string): Locator {
    return this.cards().filter({ hasText: title }).first();
  }

  async expectCard(title: string): Promise<void> {
    await expect(this.cardByTitle(title)).toBeVisible();
  }

  async expectNoCard(title: string): Promise<void> {
    await expect(this.cards().filter({ hasText: title })).toHaveCount(0);
  }

  async expectEmpty(): Promise<void> {
    await expect(this.cards()).toHaveCount(0);
  }
}
