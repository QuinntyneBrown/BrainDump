import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatMenu, MatMenuContent, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatIcon } from '@angular/material/icon';
import { BdIconButton } from '../icon-button/icon-button';
import { BdMonacoLine } from '../monaco-line/monaco-line';
import { BdButton } from '../button/button';

export interface BdEditorLine {
  readonly lineNumber: number;
  readonly code: string;
  readonly kind: 'section' | 'fact' | 'blank';
  readonly sectionId?: number;
  readonly factId?: number;
  readonly depth?: number;
  readonly level?: 0 | 1 | 2 | 3;
}

export interface SectionMenuAction {
  readonly type: 'add-fact' | 'add-child-section' | 'edit-section' | 'delete-section';
  readonly sectionId: number;
}

export interface FactMenuAction {
  readonly type: 'edit-fact' | 'delete-fact';
  readonly factId: number;
}

@Component({
  selector: 'bd-document-editor',
  imports: [
    MatIcon,
    MatMenu,
    MatMenuContent,
    MatMenuItem,
    MatMenuTrigger,
    BdIconButton,
    BdMonacoLine,
    BdButton,
  ],
  templateUrl: './document-editor.html',
  styleUrl: './document-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-active]': 'active()',
  },
})
export class BdDocumentEditor {
  readonly lines = input.required<readonly BdEditorLine[]>();
  /** When true, this is the focused pane and section/fact actions land here. */
  readonly active = input<boolean>(true);
  /** When true, render an empty-state with an "Add section" affordance. */
  readonly emptyHint = input<boolean>(true);

  readonly paneFocused = output<void>();
  readonly addRootSection = output<void>();
  readonly sectionAction = output<SectionMenuAction>();
  readonly factAction = output<FactMenuAction>();

  protected onFocus(): void {
    this.paneFocused.emit();
  }

  protected emitSection(type: SectionMenuAction['type'], sectionId: number): void {
    this.sectionAction.emit({ type, sectionId });
  }

  protected emitFact(type: FactMenuAction['type'], factId: number): void {
    this.factAction.emit({ type, factId });
  }
}
