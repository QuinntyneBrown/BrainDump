import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'bd-icon-button',
  imports: [MatIconButton, MatIcon],
  templateUrl: './icon-button.html',
  styleUrl: './icon-button.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdIconButton {
  readonly icon = input.required<string>();
  readonly disabled = input(false);
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly ariaLabel = input.required<string>();
  readonly tone = input<'default' | 'primary'>('default');
}
