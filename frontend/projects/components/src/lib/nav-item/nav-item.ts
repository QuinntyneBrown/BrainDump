import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { BdSideRail } from '../side-rail/side-rail';

@Component({
  selector: 'bd-nav-item',
  imports: [MatIcon],
  templateUrl: './nav-item.html',
  styleUrl: './nav-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-active]': 'active()',
    '[attr.data-rail]': 'inRail || null',
    '[attr.role]': '"button"',
    '[attr.tabindex]': '0',
    '[attr.aria-current]': 'active() ? "page" : null',
  },
})
export class BdNavItem {
  readonly label = input.required<string>();
  readonly icon = input.required<string>();
  readonly active = input(false);

  // When rendered inside a <bd-side-rail>, switch to a centered icon-only
  // layout so the active highlight reads as a circle around the icon.
  protected readonly inRail = inject(BdSideRail, { optional: true }) !== null;
}
