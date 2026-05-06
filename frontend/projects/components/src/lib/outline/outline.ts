import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

export interface BdOutlineEntry {
  readonly id: string | number;
  readonly label: string;
  readonly level: 1 | 2 | 3;
}

@Component({
  selector: 'bd-outline',
  imports: [MatIcon],
  templateUrl: './outline.html',
  styleUrl: './outline.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdOutline {
  readonly entries = input.required<readonly BdOutlineEntry[]>();
  readonly activeId = input<string | number | null>(null);
  readonly entryClick = output<BdOutlineEntry>();
}
