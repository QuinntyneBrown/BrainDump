import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { BdButton } from 'components';
import { AUTH_SERVICE } from 'api';

@Component({
  selector: 'app-home',
  imports: [MatIcon, BdButton],
  template: `
    <div class="home">
      <mat-icon class="home-icon">psychology</mat-icon>
      <h1>BrainDump</h1>
      <p>You're signed in. Start dumping.</p>
      <bd-button variant="outlined" (click)="signOut()">Sign Out</bd-button>
    </div>
  `,
  styles: `
    :host {
      display: flex;
      height: 100%;
      align-items: center;
      justify-content: center;
      background: var(--mat-sys-background);
    }
    .home { text-align: center; }
    .home-icon {
      font-size: 64px; width: 64px; height: 64px;
      color: var(--mat-sys-primary);
    }
    h1 {
      font: var(--mat-sys-headline-large);
      margin: 16px 0 8px;
      color: var(--mat-sys-on-surface);
    }
    p {
      font: var(--mat-sys-body-large);
      color: var(--mat-sys-on-surface-variant);
      margin: 0 0 32px;
    }
    bd-button { display: inline-block; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  private readonly authService = inject(AUTH_SERVICE);
  private readonly router = inject(Router);

  signOut(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
