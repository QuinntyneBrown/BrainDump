import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'bd-nav-item',
  imports: [MatIcon],
  templateUrl: './nav-item.html',
  styleUrl: './nav-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-active]': 'active()',
    '[attr.role]': '"button"',
    '[attr.tabindex]': '0',
    '[attr.aria-current]': 'active() ? "page" : null',
  },
})
export class BdNavItem {
  readonly label = input.required<string>();
  readonly icon = input.required<string>();
  readonly active = input(false);
}
