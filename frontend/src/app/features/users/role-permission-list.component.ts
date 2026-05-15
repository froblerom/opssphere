import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { PermissionSummary, RoleSummary } from './user-management.models';
import { UserManagementService } from './user-management.service';

@Component({
  selector: 'app-role-permission-list',
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="role-permission-view" aria-labelledby="roles-title">
      <header>
        <div>
          <p class="eyebrow">Administration</p>
          <h1 id="roles-title">Roles and Permissions</h1>
        </div>
        <a mat-stroked-button routerLink="/admin/users">
          <mat-icon aria-hidden="true">arrow_back</mat-icon>
          <span>Users</span>
        </a>
      </header>

      @if (errorMessage()) {
        <p class="error" role="alert">{{ errorMessage() }}</p>
      }

      <div class="columns">
        <section aria-labelledby="role-list-title">
          <h2 id="role-list-title">Roles</h2>
          <ul>
            @for (role of roles(); track role.id) {
              <li>
                <strong>{{ role.name }}</strong>
                <span>{{ role.description || 'No description' }}</span>
              </li>
            }
          </ul>
        </section>

        <section aria-labelledby="permission-list-title">
          <h2 id="permission-list-title">Permissions</h2>
          <ul>
            @for (permission of permissions(); track permission.id) {
              <li>
                <strong>{{ permission.code }}</strong>
                <span>{{ permission.name }}</span>
              </li>
            }
          </ul>
        </section>
      </div>
    </section>
  `,
  styles: [`
    .role-permission-view {
      display: grid;
      gap: 1rem;
    }

    header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
    }

    h1,
    h2 {
      margin: 0;
      color: #111827;
      line-height: 1.2;
    }

    h1 {
      font-size: 1.65rem;
    }

    h2 {
      font-size: 1.05rem;
    }

    .eyebrow {
      margin: 0 0 0.2rem;
      color: #5b6475;
      font-size: 0.8rem;
      font-weight: 700;
      text-transform: uppercase;
    }

    .columns {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
      align-items: start;
    }

    section section {
      display: grid;
      gap: 0.75rem;
    }

    ul {
      display: grid;
      gap: 0.55rem;
      margin: 0;
      padding: 0;
      list-style: none;
    }

    li {
      display: grid;
      gap: 0.2rem;
      border-bottom: 1px solid #e8edf5;
      padding-bottom: 0.55rem;
    }

    strong {
      color: #172033;
    }

    span {
      color: #5b6475;
    }

    .error {
      margin: 0;
      color: #b91c1c;
      font-weight: 500;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RolePermissionListComponent {
  private readonly userManagement = inject(UserManagementService);
  private readonly errorParser = inject(ApiErrorParserService);

  protected readonly roles = signal<RoleSummary[]>([]);
  protected readonly permissions = signal<PermissionSummary[]>([]);
  protected readonly errorMessage = signal('');

  constructor() {
    this.userManagement.getRoles().subscribe({
      next: (roles) => this.roles.set(roles),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });

    this.userManagement.getPermissions().subscribe({
      next: (permissions) => this.permissions.set(permissions),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }
}
