import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-admin-users',
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="admin-shell" aria-labelledby="admin-title">
      <header>
        <div>
          <p class="eyebrow">Administration</p>
          <h1 id="admin-title">User Governance</h1>
        </div>
      </header>

      <div class="admin-actions">
        <a mat-flat-button color="primary" routerLink="/admin/users">
          <mat-icon aria-hidden="true">group</mat-icon>
          <span>Users</span>
        </a>
        <a mat-stroked-button routerLink="/admin/users/create">
          <mat-icon aria-hidden="true">person_add</mat-icon>
          <span>Create User</span>
        </a>
        <a mat-stroked-button routerLink="/admin/roles">
          <mat-icon aria-hidden="true">admin_panel_settings</mat-icon>
          <span>Roles</span>
        </a>
      </div>
    </section>
  `,
  styles: [`
    .admin-shell {
      display: grid;
      gap: 1.25rem;
    }

    header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
    }

    h1 {
      margin: 0;
      color: #111827;
      font-size: 1.8rem;
      line-height: 1.2;
    }

    .eyebrow {
      margin: 0 0 0.2rem;
      color: #5b6475;
      font-size: 0.8rem;
      font-weight: 700;
      text-transform: uppercase;
    }

    .admin-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    a {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminUsersComponent {}
