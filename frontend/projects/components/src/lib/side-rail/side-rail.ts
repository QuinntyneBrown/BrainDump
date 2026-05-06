import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'bd-side-rail',
  template: `<ng-content />`,
  styleUrl: './side-rail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'navigation' },
})
export class BdSideRail {}
