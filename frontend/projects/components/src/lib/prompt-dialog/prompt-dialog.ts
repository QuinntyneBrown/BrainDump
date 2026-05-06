import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BdButton } from '../button/button';
import { BdDialog } from '../dialog/dialog';
import { BdTextArea } from '../text-area/text-area';
import { BdTextField } from '../text-field/text-field';

export interface BdPromptDialogData {
  readonly title: string;
  readonly label?: string;
  readonly placeholder?: string;
  readonly initialValue?: string;
  readonly multiline?: boolean;
  readonly confirmText?: string;
  readonly cancelText?: string;
}

@Component({
  selector: 'bd-prompt-dialog',
  imports: [BdDialog, BdTextField, BdTextArea, BdButton],
  templateUrl: './prompt-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdPromptDialog {
  protected readonly data = inject<BdPromptDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject<MatDialogRef<BdPromptDialog, string | null>>(MatDialogRef);

  protected readonly value = signal<string>(this.data.initialValue ?? '');

  protected onConfirm(): void {
    const value = this.value().trim();
    if (!value) return;
    this.ref.close(value);
  }

  protected onCancel(): void {
    this.ref.close(null);
  }
}
