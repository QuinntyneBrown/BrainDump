import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-swatch',
  templateUrl: './swatch.html',
  styleUrl: './swatch.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdSwatch {
  readonly name = input.required<string>();
  readonly color = input.required<string>();
}
