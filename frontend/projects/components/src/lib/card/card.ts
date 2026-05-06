import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-card',
  templateUrl: './card.html',
  styleUrl: './card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdCard {
  readonly title = input<string | null>(null);
  readonly body = input<string | null>(null);
}
