import { Locator, Page, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object for the multi-document tab bar introduced by Slice 03
 * (docs/detailed-designs/03-multi-document-editing).
 */
export class TabBarPage extends BasePage {
  readonly leftBar: Locator;
  readonly rightBar: Locator;
  readonly splitToggle: Locator;

  constructor(page: Page) {
    super(page);
    this.leftBar = page.locator('[data-testid="tab-bar-left"]');
    this.rightBar = page.locator('[data-testid="tab-bar-right"]');
    this.splitToggle = page.locator('[data-testid="split-toggle"]');
  }

  tabsIn(pane: 'left' | 'right'): Locator {
    return this.barFor(pane).locator('.bd-tab-bar__tab');
  }

  tabIn(pane: 'left' | 'right', title: string): Locator {
    return this.tabsIn(pane).filter({ hasText: title }).first();
  }

  async expectTabActive(pane: 'left' | 'right', title: string): Promise<void> {
    await expect(this.tabIn(pane, title)).toHaveAttribute('data-active', 'true');
  }

  async expectTabCount(pane: 'left' | 'right', count: number): Promise<void> {
    await expect(this.tabsIn(pane)).toHaveCount(count);
  }

  async clickTab(pane: 'left' | 'right', title: string): Promise<void> {
    await this.tabIn(pane, title).click();
  }

  async closeTab(pane: 'left' | 'right', title: string): Promise<void> {
    await this.tabIn(pane, title).locator('.bd-tab-bar__close').click();
  }

  async toggleSplit(): Promise<void> {
    await this.splitToggle.click();
  }

  async expectSplitActive(): Promise<void> {
    await expect(this.rightBar).toBeVisible();
  }

  async expectNotSplit(): Promise<void> {
    await expect(this.rightBar).toHaveCount(0);
  }

  private barFor(pane: 'left' | 'right'): Locator {
    return pane === 'left' ? this.leftBar : this.rightBar;
  }
}
