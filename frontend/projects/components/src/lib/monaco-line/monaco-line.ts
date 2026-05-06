import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type BdMonacoLineLevel = 0 | 1 | 2 | 3;

@Component({
  selector: 'bd-monaco-line',
  templateUrl: './monaco-line.html',
  styleUrl: './monaco-line.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-level]': 'level()',
  },
})
export class BdMonacoLine {
  readonly lineNumber = input.required<number | string>();
  readonly code = input.required<string>();
  /** 0 = body line; 1/2/3 tints the code as a markdown heading. */
  readonly level = input<BdMonacoLineLevel>(0);
}
