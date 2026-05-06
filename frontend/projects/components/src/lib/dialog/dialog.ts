import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatDialogActions, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';

export type BdDialogTone = 'default' | 'danger';

@Component({
  selector: 'bd-dialog',
  imports: [MatDialogTitle, MatDialogContent, MatDialogActions, MatIcon],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[attr.data-tone]': 'tone()' },
})
export class BdDialog {
  readonly title = input.required<string>();
  readonly body = input<string | null>(null);
  readonly icon = input<string | null>(null);
  readonly tone = input<BdDialogTone>('default');
}
