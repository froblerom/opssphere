import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <div class="page-container access-denied">
      <mat-icon aria-hidden="true">lock</mat-icon>
      <h1>Access denied</h1>
      <p>You do not have permission to access this page.</p>
      <a mat-flat-button color="primary" routerLink="/dashboard">Back to dashboard</a>
    </div>
  `,
  styles: [`
    .access-denied {
      max-width: 560px;
      text-align: center;
    }

    .access-denied mat-icon {
      color: #b45309;
      font-size: 40px;
      height: 40px;
      width: 40px;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccessDeniedComponent {}
