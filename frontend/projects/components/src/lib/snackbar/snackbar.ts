import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

export interface BdSnackbarData {
  readonly message: string;
  readonly action?: string | null;
}

@Component({
  selector: 'bd-snackbar',
  templateUrl: './snackbar.html',
  styleUrl: './snackbar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'status' },
})
export class BdSnackbar {
  readonly data = inject<BdSnackbarData>(MAT_SNACK_BAR_DATA);
  private readonly ref = inject<MatSnackBarRef<BdSnackbar>>(MatSnackBarRef);

  dismissWithAction(): void {
    this.ref.dismissWithAction();
  }
}
