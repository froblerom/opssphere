import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  template: `
    <section class="login" aria-labelledby="login-title">
      <div class="login-panel">
        <div class="login-heading">
          <mat-icon aria-hidden="true">lock</mat-icon>
          <div>
            <h1 id="login-title">Sign in to OpsSphere</h1>
            <p>Use a seeded internal account for local development.</p>
          </div>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" autocomplete="username" formControlName="email" />
            @if (form.controls.email.hasError('required')) {
              <mat-error>Email is required.</mat-error>
            }
            @if (form.controls.email.hasError('email')) {
              <mat-error>Enter a valid email address.</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" autocomplete="current-password" formControlName="password" />
            @if (form.controls.password.hasError('required')) {
              <mat-error>Password is required.</mat-error>
            }
          </mat-form-field>

          @if (errorMessage()) {
            <p class="login-error" role="alert">{{ errorMessage() }}</p>
          }

          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || isSubmitting()">
            <mat-icon aria-hidden="true">login</mat-icon>
            <span>Sign in</span>
          </button>
        </form>
      </div>
    </section>
  `,
  styles: [`
    .login {
      display: grid;
      min-height: calc(100vh - 9rem);
      place-items: center;
    }

    .login-panel {
      width: min(100%, 440px);
      border: 1px solid #d8dee8;
      border-radius: 8px;
      padding: 1.5rem;
      background: #ffffff;
    }

    .login-heading {
      display: flex;
      align-items: center;
      gap: 0.85rem;
      margin-bottom: 1.25rem;
    }

    .login-heading mat-icon {
      color: #2563eb;
    }

    h1 {
      margin: 0;
      color: #111827;
      font-size: 1.45rem;
      line-height: 1.2;
    }

    p {
      margin: 0.2rem 0 0;
      color: #4b5563;
    }

    form,
    mat-form-field {
      display: block;
      width: 100%;
    }

    .login-error {
      margin: 0 0 1rem;
      color: #b91c1c;
      font-weight: 500;
    }

    button {
      width: 100%;
    }

    button mat-icon {
      margin-right: 0.4rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly errorMessage = signal('');
  protected readonly isSubmitting = signal(false);
  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.isSubmitting.set(true);
    const { email, password } = this.form.getRawValue();

    this.authService.login(email, password).subscribe({
      next: () => {
        this.form.controls.password.reset('');
        const returnUrl = this.getSafeReturnUrl();
        void this.router.navigateByUrl(returnUrl);
      },
      error: () => {
        this.form.controls.password.reset('');
        this.errorMessage.set('Invalid email or password.');
        this.isSubmitting.set(false);
      }
    });
  }

  private getSafeReturnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    if (!returnUrl || !returnUrl.startsWith('/') || returnUrl.startsWith('//')) {
      return '/dashboard';
    }

    return returnUrl;
  }
}
