import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { UserDetail } from './user-management.models';
import { UserManagementService } from './user-management.service';

@Component({
  selector: 'app-user-detail',
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="user-detail-view" aria-labelledby="user-detail-title">
      <header>
        <div>
          <p class="eyebrow">Administration</p>
          <h1 id="user-detail-title">{{ user()?.displayName || 'User Detail' }}</h1>
        </div>
        <div class="actions">
          <a mat-stroked-button routerLink="/admin/users">
            <mat-icon aria-hidden="true">arrow_back</mat-icon>
            <span>Users</span>
          </a>
          @if (user(); as currentUser) {
            <a mat-stroked-button [routerLink]="['/admin/users', currentUser.id, 'edit']">
              <mat-icon aria-hidden="true">edit</mat-icon>
              <span>Edit</span>
            </a>
            <a mat-stroked-button [routerLink]="['/admin/users', currentUser.id, 'roles']">
              <mat-icon aria-hidden="true">manage_accounts</mat-icon>
              <span>Roles</span>
            </a>
          }
        </div>
      </header>

      @if (errorMessage()) {
        <p class="error" role="alert">{{ errorMessage() }}</p>
      }

      @if (user(); as currentUser) {
        <dl class="detail-grid">
          <div>
            <dt>Email</dt>
            <dd>{{ currentUser.email }}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd><span class="status" [class.inactive]="!currentUser.isActive">{{ currentUser.isActive ? 'Active' : 'Inactive' }}</span></dd>
          </div>
          <div>
            <dt>First name</dt>
            <dd>{{ currentUser.firstName }}</dd>
          </div>
          <div>
            <dt>Last name</dt>
            <dd>{{ currentUser.lastName }}</dd>
          </div>
          <div>
            <dt>Roles</dt>
            <dd>{{ roleNames(currentUser) }}</dd>
          </div>
          <div>
            <dt>Created</dt>
            <dd>{{ currentUser.createdAt | date:'medium' }}</dd>
          </div>
          <div>
            <dt>Last Login</dt>
            <dd>{{ currentUser.lastLoginAt ? (currentUser.lastLoginAt | date:'medium') : 'Never' }}</dd>
          </div>
          <div>
            <dt>Deactivated</dt>
            <dd>{{ currentUser.deactivatedAt ? (currentUser.deactivatedAt | date:'medium') : 'Not deactivated' }}</dd>
          </div>
        </dl>

        @if (currentUser.isActive) {
          <button mat-flat-button color="warn" type="button" (click)="deactivate(currentUser.id)" [disabled]="isSaving()">
            <mat-icon aria-hidden="true">block</mat-icon>
            <span>Deactivate</span>
          </button>
        }
      }
    </section>
  `,
  styles: [`
    .user-detail-view {
      display: grid;
      gap: 1rem;
    }

    header,
    .actions {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    header {
      justify-content: space-between;
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

    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
      margin: 0;
    }

    .detail-grid div {
      border-bottom: 1px solid #e8edf5;
      padding-bottom: 0.7rem;
    }

    dt {
      color: #5b6475;
      font-size: 0.78rem;
      font-weight: 700;
      text-transform: uppercase;
    }

    dd {
      margin: 0.25rem 0 0;
      color: #172033;
      overflow-wrap: anywhere;
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

    .error {
      margin: 0;
      color: #b91c1c;
      font-weight: 500;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserDetailComponent {
  private readonly userManagement = inject(UserManagementService);
  private readonly errorParser = inject(ApiErrorParserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly user = signal<UserDetail | null>(null);
  protected readonly errorMessage = signal('');
  protected readonly isSaving = signal(false);
  private readonly userId = this.route.snapshot.paramMap.get('id') ?? '';

  constructor() {
    this.loadUser();
  }

  protected roleNames(user: UserDetail): string {
    return user.roles.map((role) => role.name).join(', ') || 'Unassigned';
  }

  protected deactivate(id: string): void {
    this.errorMessage.set('');
    this.isSaving.set(true);
    this.userManagement.deactivateUser(id).subscribe({
      next: () => this.userManagement.getUser(id).subscribe({
        next: (user) => {
          this.user.set(user);
          this.isSaving.set(false);
        },
        error: (error) => {
          this.errorMessage.set(this.errorParser.parse(error).message);
          this.isSaving.set(false);
        }
      }),
      error: (error) => {
        this.errorMessage.set(this.errorParser.parse(error).message);
        this.isSaving.set(false);
      }
    });
  }

  private loadUser(): void {
    if (!this.userId) {
      void this.router.navigate(['/admin/users']);
      return;
    }

    this.userManagement.getUser(this.userId).subscribe({
      next: (user) => this.user.set(user),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }
}
