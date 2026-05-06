import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'bd-list-item',
  imports: [MatIcon],
  templateUrl: './list-item.html',
  styleUrl: './list-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'listitem' },
})
export class BdListItem {
  readonly title = input.required<string>();
  readonly meta = input<string | null>(null);
  readonly leadingIcon = input<string | null>(null);
  readonly trailingIcon = input<string | null>('chevron_right');
}
