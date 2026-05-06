import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatFabButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'bd-fab',
  imports: [MatFabButton, MatIcon],
  templateUrl: './fab.html',
  styleUrl: './fab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdFab {
  readonly icon = input.required<string>();
  readonly disabled = input(false);
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly ariaLabel = input.required<string>();
}
