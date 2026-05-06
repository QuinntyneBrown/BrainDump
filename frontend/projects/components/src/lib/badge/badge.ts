import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type BdBadgeTone = 'primary' | 'error' | 'success' | 'warning';

@Component({
  selector: 'bd-badge',
  templateUrl: './badge.html',
  styleUrl: './badge.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-tone]': 'tone()',
    role: 'status',
  },
})
export class BdBadge {
  readonly count = input<number | string>(0);
  readonly max = input<number>(99);
  readonly tone = input<BdBadgeTone>('primary');

  readonly display = computed(() => {
    const value = this.count();
    if (typeof value === 'string') return value;
    return value > this.max() ? `${this.max()}+` : `${value}`;
  });
}
