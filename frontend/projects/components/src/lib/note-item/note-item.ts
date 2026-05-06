import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-note-item',
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
  readonly active = input(false);
}
