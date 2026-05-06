import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

export interface BdTab {
  readonly id: number;
  readonly title: string;
}

@Component({
  selector: 'bd-tab-bar',
  imports: [MatIcon],
  templateUrl: './tab-bar.html',
  styleUrl: './tab-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdTabBar {
  readonly tabs = input.required<readonly BdTab[]>();
  readonly activeIndex = input<number>(-1);
  readonly canSplit = input<boolean>(true);
  readonly splitActive = input<boolean>(false);

  readonly tabSelected = output<number>();
  readonly tabClosed = output<number>();
  readonly splitToggled = output<void>();

  protected onTabClick(idx: number, event: MouseEvent): void {
    // Middle-click closes the tab — common browser convention.
    if (event.button === 1) {
      event.preventDefault();
      this.tabClosed.emit(idx);
      return;
    }
    this.tabSelected.emit(idx);
  }

  protected onClose(idx: number, event: MouseEvent): void {
    event.stopPropagation();
    this.tabClosed.emit(idx);
  }

  protected onSplit(): void {
    this.splitToggled.emit();
  }
}
