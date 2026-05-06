import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

export type BdStatusBarSaveState = 'saved' | 'saving' | 'dirty' | 'error';

export interface BdStatusBarLeft {
  readonly saveState: BdStatusBarSaveState;
  readonly savedAgo?: string | null;
  readonly branch?: string | null;
}

export interface BdStatusBarRight {
  readonly lines: number;
  readonly language: string;
  readonly encoding: string;
  readonly cursor: { line: number; col: number };
}

@Component({
  selector: 'bd-status-bar',
  imports: [MatIcon],
  templateUrl: './status-bar.html',
  styleUrl: './status-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdStatusBar {
  readonly left = input.required<BdStatusBarLeft>();
  readonly right = input.required<BdStatusBarRight>();

  protected readonly saveIcon = computed(() => {
    switch (this.left().saveState) {
      case 'saved': return 'check_circle';
      case 'saving': return 'sync';
      case 'dirty': return 'edit';
      case 'error': return 'error';
    }
  });

  protected readonly saveLabel = computed(() => {
    const l = this.left();
    if (l.saveState === 'saved') return l.savedAgo ? `Saved ${l.savedAgo}` : 'Saved';
    if (l.saveState === 'saving') return 'Saving…';
    if (l.saveState === 'dirty') return 'Unsaved changes';
    return 'Save failed';
  });
}
