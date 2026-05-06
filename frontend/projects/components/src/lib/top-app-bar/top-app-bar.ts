import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatToolbar } from '@angular/material/toolbar';
import { BdIconButton } from '../icon-button/icon-button';

export interface BdTopAppBarAction {
  readonly icon: string;
  readonly ariaLabel: string;
  readonly id?: string;
}

@Component({
  selector: 'bd-top-app-bar',
  imports: [MatToolbar, BdIconButton],
  templateUrl: './top-app-bar.html',
  styleUrl: './top-app-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdTopAppBar {
  readonly title = input.required<string>();
  readonly leadingIcon = input<string | null>('menu');
  readonly leadingAriaLabel = input<string>('Open navigation');
  readonly actions = input<readonly BdTopAppBarAction[]>([]);

  readonly leadingClick = output<void>();
  readonly actionClick = output<BdTopAppBarAction>();
}
