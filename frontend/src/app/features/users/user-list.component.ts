import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { UserSummary } from './user-management.models';
import { UserManagementService } from './user-management.service';

@Component({
  selector: 'app-user-list',
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="users-view" aria-labelledby="users-title">
      <header>
        <div>
          <p class="eyebrow">Administration</p>
          <h1 id="users-title">Users</h1>
        </div>
        <a mat-flat-button color="primary" routerLink="/admin/users/create">
          <mat-icon aria-hidden="true">person_add</mat-icon>
          <span>Create</span>
        </a>
      </header>

      @if (errorMessage()) {
        <p class="error" role="alert">{{ errorMessage() }}</p>
      }

      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Roles</th>
              <th>Status</th>
              <th>Last Login</th>
              <th aria-label="Actions"></th>
            </tr>
          </thead>
          <tbody>
            @for (user of users(); track user.id) {
              <tr>
                <td>
                  <a class="name-link" [routerLink]="['/admin/users', user.id]">{{ user.displayName }}</a>
                </td>
                <td>{{ user.email }}</td>
                <td>{{ roleNames(user) }}</td>
                <td><span class="status" [class.inactive]="!user.isActive">{{ user.isActive ? 'Active' : 'Inactive' }}</span></td>
                <td>{{ user.lastLoginAt ? (user.lastLoginAt | date:'medium') : 'Never' }}</td>
                <td class="actions">
                  <a mat-icon-button [routerLink]="['/admin/users', user.id, 'edit']" aria-label="Edit user">
                    <mat-icon aria-hidden="true">edit</mat-icon>
                  </a>
                  <a mat-icon-button [routerLink]="['/admin/users', user.id, 'roles']" aria-label="Assign roles">
                    <mat-icon aria-hidden="true">manage_accounts</mat-icon>
                  </a>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6" class="empty">No users found.</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .users-view {
      display: grid;
      gap: 1rem;
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
      font-size: 1.65rem;
      line-height: 1.2;
    }

    .eyebrow {
      margin: 0 0 0.2rem;
      color: #5b6475;
      font-size: 0.8rem;
      font-weight: 700;
      text-transform: uppercase;
    }

    .table-wrap {
      overflow: auto;
      border: 1px solid #d8dee8;
      border-radius: 8px;
      background: #ffffff;
    }

    table {
      width: 100%;
      min-width: 760px;
      border-collapse: collapse;
    }

    th,
    td {
      padding: 0.8rem 0.9rem;
      border-bottom: 1px solid #e8edf5;
      text-align: left;
      vertical-align: middle;
    }

    th {
      color: #3d4758;
      font-size: 0.78rem;
      font-weight: 700;
      text-transform: uppercase;
    }

    tr:last-child td {
      border-bottom: 0;
    }

    .name-link {
      color: #1d4ed8;
      font-weight: 600;
      text-decoration: none;
    }

    .status {
      display: inline-block;
      min-width: 4.6rem;
      border-radius: 999px;
      padding: 0.2rem 0.55rem;
      background: #e8f6ef;
      color: #166534;
      text-align: center;
      font-size: 0.78rem;
      font-weight: 700;
    }

    .status.inactive {
      background: #f2f4f7;
      color: #5b6475;
    }

    .actions {
      width: 7rem;
      white-space: nowrap;
    }

    .empty,
    .error {
      color: #7f1d1d;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserListComponent {
  private readonly userManagement = inject(UserManagementService);
  private readonly errorParser = inject(ApiErrorParserService);

  protected readonly users = signal<UserSummary[]>([]);
  protected readonly errorMessage = signal('');
  protected readonly activeUserCount = computed(() => this.users().filter((u) => u.isActive).length);

  constructor() {
    this.loadUsers();
  }

  protected roleNames(user: UserSummary): string {
    return user.roles.map((role) => role.name).join(', ') || 'Unassigned';
  }

  private loadUsers(): void {
    this.userManagement.getUsers().subscribe({
      next: (users) => this.users.set(users),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }
}
