import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'bd-note-item',
  imports: [MatIcon],
  templateUrl: './note-item.html',
  styleUrl: './note-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-active]': 'active()',
    '[attr.role]': '"button"',
    '[attr.tabindex]': '0',
    '[attr.aria-current]': 'active() ? "true" : null',
  },
})
export class BdNoteItem {
  readonly title = input.required<string>();
  readonly preview = input<string | null>(null);
  readonly meta = input<string | null>(null);
  readonly tags = input<readonly string[]>([]);
  readonly icon = input<string>('description');
  readonly active = input(false);
}
