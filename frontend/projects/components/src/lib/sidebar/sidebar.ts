import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-sidebar',
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'complementary' },
})
export class BdSidebar {
  readonly heading = input<string | null>(null);
}
