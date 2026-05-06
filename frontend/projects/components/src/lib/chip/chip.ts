import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-chip',
  templateUrl: './chip.html',
  styleUrl: './chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-selected]': 'selected()',
    '[attr.data-disabled]': 'disabled() || null',
    '[attr.role]': '"button"',
    '[attr.tabindex]': 'disabled() ? -1 : 0',
    '[attr.aria-pressed]': 'selected()',
  },
})
export class BdChip {
  readonly selected = input(false);
  readonly disabled = input(false);
}
