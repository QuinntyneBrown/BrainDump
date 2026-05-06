import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bd-split-editor',
  template: `<ng-content />`,
  styleUrl: './split-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.data-split]': 'split()',
  },
})
export class BdSplitEditor {
  readonly split = input(false);
}
