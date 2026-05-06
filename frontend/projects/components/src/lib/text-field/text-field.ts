import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'bd-text-field',
  imports: [FormsModule, MatFormFieldModule, MatInput, MatIcon],
  templateUrl: './text-field.html',
  styleUrl: './text-field.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdTextField {
  readonly value = model<string>('');
  readonly label = input<string | null>(null);
  readonly placeholder = input<string>('');
  readonly leadingIcon = input<string | null>('search');
  readonly trailingIcon = input<string | null>(null);
  readonly type = input<'text' | 'password' | 'email' | 'number' | 'search'>('text');
  readonly disabled = input(false);
  readonly hint = input<string | null>(null);
}
