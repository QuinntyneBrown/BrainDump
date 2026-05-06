import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { MatToolbar } from '@angular/material/toolbar';
import { BdBadge } from '../badge/badge';
import { BdIconButton } from '../icon-button/icon-button';

export interface BdTopAppBarAction {
  readonly icon: string;
  readonly ariaLabel: string;
  readonly id?: string;
}

@Component({
  selector: 'bd-top-app-bar',
  imports: [MatToolbar, MatIcon, BdBadge, BdIconButton],
  templateUrl: './top-app-bar.html',
  styleUrl: './top-app-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdTopAppBar {
  readonly title = input.required<string>();
  readonly leadingIcon = input<string | null>('menu');
  readonly leadingAriaLabel = input<string>('Open navigation');
  /** Non-interactive glyph rendered between the optional leading button and the title. */
  readonly decorativeIcon = input<string | null>(null);
  /** Inline badge label sitting between the title and the caption. */
  readonly badge = input<string | null>(null);
  /** Trailing tertiary caption (e.g. "Edited 2 minutes ago"). */
  readonly caption = input<string | null>(null);
  readonly actions = input<readonly BdTopAppBarAction[]>([]);

  readonly leadingClick = output<void>();
  readonly actionClick = output<BdTopAppBarAction>();
}
