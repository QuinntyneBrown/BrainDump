import { Locator, Page, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object for the "Recently viewed" sidebar section introduced by
 * Slice 04 (docs/detailed-designs/04-recent-activity).
 */
export class RecentsPage extends BasePage {
  readonly recents: Locator;

  constructor(page: Page) {
    super(page);
    this.recents = page.locator('[data-testid="recents"]');
  }

  rows(): Locator {
    return this.recents.locator('.home-shell__recent-row');
  }

  rowByTitle(title: string): Locator {
    return this.rows().filter({ hasText: title }).first();
  }

  async expectOrder(titles: readonly string[]): Promise<void> {
    await expect(this.rows()).toHaveCount(titles.length);
    for (let i = 0; i < titles.length; i++) {
      await expect(this.rows().nth(i)).toContainText(titles[i]);
    }
  }

  async expectVisible(title: string): Promise<void> {
    await expect(this.rowByTitle(title)).toBeVisible();
  }

  async expectAbsent(title: string): Promise<void> {
    await expect(this.recents.locator('.home-shell__recent-row', { hasText: title })).toHaveCount(0);
  }

  async clickRow(title: string): Promise<void> {
    await this.rowByTitle(title).click();
  }
}
