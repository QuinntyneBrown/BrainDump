import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatChip, MatChipRemove } from '@angular/material/chips';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'bd-chip',
  imports: [MatChip, MatChipRemove, MatIcon],
  templateUrl: './chip.html',
  styleUrl: './chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdChip {
  readonly label = input.required<string>();
  readonly icon = input<string | null>(null);
  readonly removable = input(false);
  readonly disabled = input(false);
  readonly removed = output<void>();
}
