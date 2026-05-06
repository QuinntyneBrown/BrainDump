import { Locator, Page, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class HomePage extends BasePage {
  readonly editor: Locator;
  readonly addSectionAside: Locator;
  readonly topBarTitle: Locator;
  readonly signOutAction: Locator;

  constructor(page: Page) {
    super(page);
    this.editor = page.locator('[data-testid="editor"]');
    this.addSectionAside = page.locator('[data-testid="add-section"] button');
    this.topBarTitle = page.locator('.bd-top-app-bar__title');
    this.signOutAction = page.locator('button[aria-label="Sign out"]');
  }

  async waitForLoaded(): Promise<void> {
    await this.page.waitForURL('**/home');
    await expect(this.topBarTitle).toContainText('brain-dump.md');
    await expect(this.editor).toBeVisible();
    // Loading indicator clears
    await expect(this.page.locator('.home-shell__loading')).toHaveCount(0);
  }

  sectionLine(sectionId: number): Locator {
    return this.page.locator(`.home-shell__line[data-section-id="${sectionId}"]`);
  }

  factLine(factId: number): Locator {
    return this.page.locator(`.home-shell__line[data-fact-id="${factId}"]`);
  }

  allSectionLines(): Locator {
    return this.page.locator('.home-shell__line[data-kind="section"]');
  }

  allFactLines(): Locator {
    return this.page.locator('.home-shell__line[data-kind="fact"]');
  }

  lineByText(text: string): Locator {
    return this.page.locator('.home-shell__line', {
      has: this.page.locator('.bd-monaco-line__code', { hasText: text }),
    });
  }

  async sectionIdForTitle(title: string): Promise<number> {
    const line = this.lineByText(title).filter({ has: this.page.locator('[data-kind="section"]') }).first();
    const altLine = this.allSectionLines().filter({ hasText: title }).first();
    const target = await altLine.count() > 0 ? altLine : line;
    const id = await target.getAttribute('data-section-id');
    if (!id) throw new Error(`No section line found for title "${title}"`);
    return Number(id);
  }

  async factIdForText(text: string): Promise<number> {
    const line = this.allFactLines().filter({ hasText: text }).first();
    const id = await line.getAttribute('data-fact-id');
    if (!id) throw new Error(`No fact line found for text "${text}"`);
    return Number(id);
  }

  async openSectionMenuFromEditor(sectionId: number): Promise<void> {
    const line = this.sectionLine(sectionId);
    await line.hover();
    await line.locator(`[data-testid="section-menu-${sectionId}"] button`).click();
  }

  async openFactMenuFromEditor(factId: number): Promise<void> {
    const line = this.factLine(factId);
    await line.hover();
    await line.locator(`[data-testid="fact-menu-${factId}"] button`).click();
  }

  async clickMenuItem(testId: string): Promise<void> {
    await this.page.locator(`[data-testid="${testId}"]`).click();
  }

  async addRootSection(title: string): Promise<void> {
    await this.addSectionAside.click();
    await this.fillPromptDialog(title);
  }

  async addFactToSection(sectionId: number, text: string): Promise<void> {
    await this.openSectionMenuFromEditor(sectionId);
    await this.clickMenuItem('menu-add-fact');
    await this.fillPromptDialog(text, { multiline: true });
  }

  async addChildSection(parentSectionId: number, title: string): Promise<void> {
    await this.openSectionMenuFromEditor(parentSectionId);
    await this.clickMenuItem('menu-add-child-section');
    await this.fillPromptDialog(title);
  }

  async renameSection(sectionId: number, newTitle: string): Promise<void> {
    await this.openSectionMenuFromEditor(sectionId);
    await this.clickMenuItem('menu-edit-section');
    await this.clearPromptDialog();
    await this.fillPromptDialog(newTitle);
  }

  async deleteSection(sectionId: number): Promise<void> {
    await this.openSectionMenuFromEditor(sectionId);
    await this.clickMenuItem('menu-delete-section');
    await this.confirmDialog();
  }

  async editFact(factId: number, newText: string): Promise<void> {
    await this.openFactMenuFromEditor(factId);
    await this.clickMenuItem('menu-edit-fact');
    await this.clearPromptDialog({ multiline: true });
    await this.fillPromptDialog(newText, { multiline: true });
  }

  async deleteFact(factId: number): Promise<void> {
    await this.openFactMenuFromEditor(factId);
    await this.clickMenuItem('menu-delete-fact');
    await this.confirmDialog();
  }

  async signOut(): Promise<void> {
    await this.signOutAction.click();
    await this.page.waitForURL('**/login');
  }

  private async fillPromptDialog(text: string, opts: { multiline?: boolean } = {}): Promise<void> {
    const input = opts.multiline
      ? this.page.locator('.cdk-overlay-container textarea')
      : this.page.locator('.cdk-overlay-container input');
    await input.waitFor({ state: 'visible' });
    await input.fill(text);
    await this.page.locator('.cdk-overlay-container button:has-text("Save")').click();
    await this.page.locator('.cdk-overlay-container .bd-dialog__title').waitFor({ state: 'detached' });
  }

  private async clearPromptDialog(opts: { multiline?: boolean } = {}): Promise<void> {
    const input = opts.multiline
      ? this.page.locator('.cdk-overlay-container textarea')
      : this.page.locator('.cdk-overlay-container input');
    await input.waitFor({ state: 'visible' });
    await input.fill('');
  }

  private async confirmDialog(): Promise<void> {
    await this.page.locator('.cdk-overlay-container button:has-text("Delete")').click();
    await this.page.locator('.cdk-overlay-container .bd-dialog__title').waitFor({ state: 'detached' });
  }
}
