import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BdButton } from '../button/button';
import { BdDialog, BdDialogTone } from '../dialog/dialog';

export interface BdConfirmDialogData {
  readonly title: string;
  readonly body?: string;
  readonly icon?: string;
  readonly tone?: BdDialogTone;
  readonly confirmText?: string;
  readonly cancelText?: string;
}

@Component({
  selector: 'bd-confirm-dialog',
  imports: [BdDialog, BdButton],
  templateUrl: './confirm-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdConfirmDialog {
  protected readonly data = inject<BdConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject<MatDialogRef<BdConfirmDialog, boolean>>(MatDialogRef);

  protected onConfirm(): void {
    this.ref.close(true);
  }

  protected onCancel(): void {
    this.ref.close(false);
  }
}
