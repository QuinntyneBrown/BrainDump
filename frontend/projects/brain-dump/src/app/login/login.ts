import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { BdTextField, BdButton } from 'components';
import { AUTH_SERVICE } from 'api';

@Component({
  selector: 'app-login',
  imports: [MatIcon, BdTextField, BdButton],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login {
  private readonly authService = inject(AUTH_SERVICE);
  private readonly router = inject(Router);

  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (!this.email() || !this.password()) {
      this.error.set('Please enter your email and password.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.authService.login(this.email(), this.password()).subscribe({
      next: () => this.router.navigate(['/home']),
      error: () => {
        this.error.set('Invalid email or password. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
