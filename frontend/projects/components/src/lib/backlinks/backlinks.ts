import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

export interface BdBacklinkEntry {
  readonly id: string | number;
  readonly title: string;
  readonly excerpt: string;
}

@Component({
  selector: 'bd-backlinks',
  templateUrl: './backlinks.html',
  styleUrl: './backlinks.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdBacklinks {
  readonly entries = input.required<readonly BdBacklinkEntry[]>();
  readonly entryClick = output<BdBacklinkEntry>();
}
