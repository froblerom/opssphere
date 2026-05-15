import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { RoleSummary, UserDetail } from './user-management.models';
import { UserManagementService } from './user-management.service';

@Component({
  selector: 'app-user-roles',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatCheckboxModule, MatIconModule],
  template: `
    <section class="user-roles-view" aria-labelledby="user-roles-title">
      <header>
        <div>
          <p class="eyebrow">Administration</p>
          <h1 id="user-roles-title">Assign Roles</h1>
        </div>
        <a mat-stroked-button routerLink="/admin/users">
          <mat-icon aria-hidden="true">arrow_back</mat-icon>
          <span>Users</span>
        </a>
      </header>

      @if (user(); as currentUser) {
        <p class="subject">{{ currentUser.displayName }} · {{ currentUser.email }}</p>
      }

      @if (errorMessage()) {
        <p class="error" role="alert">{{ errorMessage() }}</p>
      }

      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <fieldset>
          <legend>Roles</legend>
          @for (role of roles(); track role.id) {
            <mat-checkbox [checked]="selectedRoleIds().includes(role.id)" [disabled]="!role.isActive" (change)="toggleRole(role.id, $event.checked)">
              {{ role.name }}
            </mat-checkbox>
          } @empty {
            <p class="empty">No active roles available.</p>
          }
        </fieldset>

        @if (form.controls.roleIds.hasError('required')) {
          <p class="error" role="alert">At least one role is required.</p>
        }

        <div class="actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || isSaving()">
            <mat-icon aria-hidden="true">save</mat-icon>
            <span>Save</span>
          </button>
          <a mat-button [routerLink]="['/admin/users', userId]">Cancel</a>
        </div>
      </form>
    </section>
  `,
  styles: [`
    .user-roles-view {
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

    .subject {
      margin: 0;
      color: #3d4758;
    }

    fieldset {
      display: grid;
      max-width: 560px;
      gap: 0.65rem;
      border: 1px solid #d8dee8;
      border-radius: 8px;
      margin: 0 0 1rem;
      padding: 1rem;
    }

    legend {
      color: #3d4758;
      font-weight: 700;
    }

    .error,
    .empty {
      margin: 0;
      color: #b91c1c;
      font-weight: 500;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserRolesComponent {
  private readonly userManagement = inject(UserManagementService);
  private readonly errorParser = inject(ApiErrorParserService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly userId = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly user = signal<UserDetail | null>(null);
  protected readonly roles = signal<RoleSummary[]>([]);
  protected readonly selectedRoleIds = signal<string[]>([]);
  protected readonly errorMessage = signal('');
  protected readonly isSaving = signal(false);
  protected readonly form = this.formBuilder.nonNullable.group({
    roleIds: [new Array<string>(), [Validators.required]]
  });

  constructor() {
    this.load();
  }

  protected toggleRole(roleId: string, checked: boolean): void {
    const next = checked
      ? Array.from(new Set([...this.selectedRoleIds(), roleId]))
      : this.selectedRoleIds().filter((id) => id !== roleId);

    this.selectedRoleIds.set(next);
    this.form.controls.roleIds.setValue(next);
    this.form.controls.roleIds.markAsDirty();
  }

  protected submit(): void {
    if (this.form.invalid || this.selectedRoleIds().length === 0) {
      this.form.markAllAsTouched();
      this.form.controls.roleIds.setErrors({ required: true });
      return;
    }

    this.errorMessage.set('');
    this.isSaving.set(true);
    this.userManagement.updateUserRoles(this.userId, this.selectedRoleIds()).subscribe({
      next: (user) => void this.router.navigate(['/admin/users', user.id]),
      error: (error) => {
        this.errorMessage.set(this.errorParser.parse(error).message);
        this.isSaving.set(false);
      }
    });
  }

  private load(): void {
    if (!this.userId) {
      void this.router.navigate(['/admin/users']);
      return;
    }

    this.userManagement.getRoles().subscribe({
      next: (roles) => this.roles.set(roles),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });

    this.userManagement.getUser(this.userId).subscribe({
      next: (user) => {
        this.user.set(user);
        const roleIds = user.roles.map((role) => role.id);
        this.selectedRoleIds.set(roleIds);
        this.form.controls.roleIds.setValue(roleIds);
      },
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }
}
