import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { SafeApiError } from '../../core/models/api-error.models';
import { CustomerService } from './customer.service';
import { OrganizationService } from '../organization/organization.service';
import { Account } from '../organization/organization.models';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="page-container">
      <h1>{{ isEdit ? 'Edit Customer' : 'New Customer' }}</h1>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-group">
          <label for="accountId">Account</label>
          <select id="accountId" formControlName="accountId">
            <option value="">Select an account</option>
            <option *ngFor="let account of accounts" [value]="account.id">{{ account.name }} ({{ account.code }})</option>
          </select>
          <div *ngIf="fieldError('accountId')" class="field-error">{{ fieldError('accountId') }}</div>
        </div>

        <div class="form-group">
          <label for="firstName">First Name</label>
          <input id="firstName" formControlName="firstName" type="text" maxlength="100" />
          <div *ngIf="fieldError('firstName')" class="field-error">{{ fieldError('firstName') }}</div>
        </div>

        <div class="form-group">
          <label for="lastName">Last Name</label>
          <input id="lastName" formControlName="lastName" type="text" maxlength="100" />
          <div *ngIf="fieldError('lastName')" class="field-error">{{ fieldError('lastName') }}</div>
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" formControlName="email" type="email" maxlength="256" />
          <div *ngIf="fieldError('email')" class="field-error">{{ fieldError('email') }}</div>
        </div>

        <div class="form-group">
          <label for="phoneNumber">Phone Number</label>
          <input id="phoneNumber" formControlName="phoneNumber" type="text" maxlength="50" />
          <div *ngIf="fieldError('phoneNumber')" class="field-error">{{ fieldError('phoneNumber') }}</div>
        </div>

        <div class="form-group">
          <label for="externalReference">External Reference</label>
          <input id="externalReference" formControlName="externalReference" type="text" maxlength="100" />
          <div *ngIf="fieldError('externalReference')" class="field-error">{{ fieldError('externalReference') }}</div>
        </div>

        <div *ngIf="generalError" class="error">{{ generalError }}</div>

        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="submitting">
            {{ submitting ? 'Saving...' : 'Save' }}
          </button>
          <a routerLink="/customers" class="btn btn-secondary">Cancel</a>
        </div>
      </form>
    </div>
  `
})
export class CustomerFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly organizationService = inject(OrganizationService);
  private readonly errorParser = inject(ApiErrorParserService);

  isEdit = false;
  customerId: string | null = null;
  accounts: Account[] = [];
  submitting = false;
  generalError: string | null = null;
  fieldErrors: Record<string, string> = {};

  form = this.fb.group({
    accountId: ['', Validators.required],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.email, Validators.maxLength(256)]],
    phoneNumber: ['', Validators.maxLength(50)],
    externalReference: ['', Validators.maxLength(100)]
  });

  fieldError(name: string): string | null {
    return this.fieldErrors[name] ?? null;
  }

  ngOnInit() {
    this.customerId = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.customerId;

    this.organizationService.getAccounts().subscribe((accounts) => {
      this.accounts = accounts.filter((a) => a.isActive);
    });

    if (this.isEdit) {
      this.customerService.getCustomer(this.customerId!).subscribe({
        next: (customer) => {
          this.form.patchValue({
            accountId: customer.accountId,
            firstName: customer.firstName,
            lastName: customer.lastName,
            email: customer.email ?? '',
            phoneNumber: customer.phoneNumber ?? '',
            externalReference: customer.externalReference ?? ''
          });
        },
        error: () => {
          this.generalError = 'Customer not found.';
        }
      });
    }
  }

  submit() {
    if (this.form.invalid || this.submitting) return;
    this.submitting = true;
    this.generalError = null;
    this.fieldErrors = {};

    const value = this.form.getRawValue();
    const request = {
      accountId: value.accountId!,
      firstName: value.firstName!,
      lastName: value.lastName!,
      email: value.email || null,
      phoneNumber: value.phoneNumber || null,
      externalReference: value.externalReference || null
    };

    const operation = this.isEdit
      ? this.customerService.updateCustomer(this.customerId!, request)
      : this.customerService.createCustomer(request);

    operation.subscribe({
      next: (customer) => this.router.navigate(['/customers', customer.id]),
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.fieldErrors = Object.fromEntries(
          parsed.details.filter((d) => d.field).map((d) => [d.field!, d.message])
        );
        this.generalError = parsed.details.length === 0 ? parsed.message : null;
        this.submitting = false;
      }
    });
  }
}
