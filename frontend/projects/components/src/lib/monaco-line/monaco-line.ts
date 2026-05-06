import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-monaco-line',
  templateUrl: './monaco-line.html',
  styleUrl: './monaco-line.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdMonacoLine {
  readonly lineNumber = input.required<number | string>();
  readonly code = input.required<string>();
}
