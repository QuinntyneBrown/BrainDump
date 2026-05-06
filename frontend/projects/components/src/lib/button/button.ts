import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

export type BdButtonVariant = 'filled' | 'tonal' | 'outlined' | 'text';

@Component({
  selector: 'bd-button',
  imports: [MatButton, MatIcon],
  templateUrl: './button.html',
  styleUrl: './button.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[attr.data-variant]': 'variant()' },
})
export class BdButton {
  readonly variant = input<BdButtonVariant>('filled');
  readonly icon = input<string | null>(null);
  readonly trailingIcon = input<string | null>(null);
  readonly disabled = input(false);
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly ariaLabel = input<string | null>(null);
}
