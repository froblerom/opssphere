import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { UserManagementService } from './user-management.service';

@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  template: `
    <section class="user-form-view" aria-labelledby="user-form-title">
      <header>
        <div>
          <p class="eyebrow">Administration</p>
          <h1 id="user-form-title">{{ isEditMode() ? 'Edit User' : 'Create User' }}</h1>
        </div>
        <a mat-stroked-button routerLink="/admin/users">
          <mat-icon aria-hidden="true">arrow_back</mat-icon>
          <span>Users</span>
        </a>
      </header>

      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput type="email" autocomplete="off" formControlName="email" />
          @if (form.controls.email.hasError('required')) {
            <mat-error>Email is required.</mat-error>
          }
          @if (form.controls.email.hasError('email')) {
            <mat-error>Enter a valid email address.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>First name</mat-label>
          <input matInput autocomplete="off" formControlName="firstName" />
          @if (form.controls.firstName.hasError('required')) {
            <mat-error>First name is required.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Last name</mat-label>
          <input matInput autocomplete="off" formControlName="lastName" />
          @if (form.controls.lastName.hasError('required')) {
            <mat-error>Last name is required.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Display name</mat-label>
          <input matInput autocomplete="off" formControlName="displayName" />
        </mat-form-field>

        @if (!isEditMode()) {
          <mat-form-field appearance="outline">
            <mat-label>Temporary password</mat-label>
            <input matInput type="password" autocomplete="new-password" formControlName="temporaryPassword" />
            @if (form.controls.temporaryPassword.hasError('required')) {
              <mat-error>Temporary password is required.</mat-error>
            }
            @if (form.controls.temporaryPassword.hasError('minlength')) {
              <mat-error>Use at least 12 characters.</mat-error>
            }
          </mat-form-field>
        }

        @if (errorMessage()) {
          <p class="error" role="alert">{{ errorMessage() }}</p>
        }

        <div class="actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || isSaving()">
            <mat-icon aria-hidden="true">save</mat-icon>
            <span>Save</span>
          </button>
          <a mat-button routerLink="/admin/users">Cancel</a>
        </div>
      </form>
    </section>
  `,
  styles: [`
    .user-form-view {
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

    form {
      display: grid;
      max-width: 640px;
      gap: 0.75rem;
    }

    .actions {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .error {
      margin: 0;
      color: #b91c1c;
      font-weight: 500;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserFormComponent {
  private readonly userManagement = inject(UserManagementService);
  private readonly errorParser = inject(ApiErrorParserService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly errorMessage = signal('');
  protected readonly isSaving = signal(false);
  protected readonly userId = this.route.snapshot.paramMap.get('id');
  protected readonly isEditMode = signal(Boolean(this.userId));

  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    displayName: ['', [Validators.maxLength(200)]],
    temporaryPassword: ['', this.isEditMode() ? [] : [Validators.required, Validators.minLength(12)]]
  });

  constructor() {
    if (this.userId) {
      this.userManagement.getUser(this.userId).subscribe({
        next: (user) => this.form.patchValue({
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
          displayName: user.displayName,
          temporaryPassword: ''
        }),
        error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
      });
    }
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.isSaving.set(true);
    const raw = this.form.getRawValue();
    const displayName = raw.displayName.trim() || `${raw.firstName.trim()} ${raw.lastName.trim()}`;

    const request$ = this.userId
      ? this.userManagement.updateUser(this.userId, {
        email: raw.email.trim(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        displayName
      })
      : this.userManagement.createUser({
        email: raw.email.trim(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        displayName,
        temporaryPassword: raw.temporaryPassword
      });

    request$.subscribe({
      next: (user) => void this.router.navigate(['/admin/users', user.id]),
      error: (error) => {
        this.errorMessage.set(this.errorParser.parse(error).message);
        this.isSaving.set(false);
      }
    });
  }
}
