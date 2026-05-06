import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'bd-text-area',
  imports: [FormsModule, MatFormFieldModule, MatInput],
  templateUrl: './text-area.html',
  styleUrl: './text-area.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdTextArea {
  readonly value = model<string>('');
  readonly label = input<string | null>(null);
  readonly placeholder = input<string>('');
  readonly rows = input<number>(6);
  readonly disabled = input(false);
  readonly hint = input<string | null>(null);
}
