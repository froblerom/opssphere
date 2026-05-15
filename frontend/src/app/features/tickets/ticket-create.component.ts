import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { SafeApiError } from '../../core/models/api-error.models';
import { CustomerService } from '../customers/customer.service';
import { OrganizationService } from '../organization/organization.service';
import { TicketService } from './ticket.service';
import { Customer } from '../customers/customer.models';
import { Account, Campaign } from '../organization/organization.models';

@Component({
  selector: 'app-ticket-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="page-container">
      <h1>New Ticket</h1>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-group">
          <label for="customerId">Customer</label>
          <select id="customerId" formControlName="customerId">
            <option value="">Select a customer</option>
            <option *ngFor="let customer of customers" [value]="customer.id">
              {{ customer.firstName }} {{ customer.lastName }}
            </option>
          </select>
          <div *ngIf="fieldError('customerId')" class="field-error">{{ fieldError('customerId') }}</div>
        </div>

        <div class="form-group">
          <label for="accountId">Account</label>
          <select id="accountId" formControlName="accountId">
            <option value="">Select an account</option>
            <option *ngFor="let account of accounts" [value]="account.id">{{ account.name }} ({{ account.code }})</option>
          </select>
          <div *ngIf="fieldError('accountId')" class="field-error">{{ fieldError('accountId') }}</div>
        </div>

        <div class="form-group">
          <label for="campaignId">Campaign</label>
          <select id="campaignId" formControlName="campaignId">
            <option value="">Select a campaign</option>
            <option *ngFor="let campaign of campaigns" [value]="campaign.id">{{ campaign.name }} ({{ campaign.code }})</option>
          </select>
          <div *ngIf="fieldError('campaignId')" class="field-error">{{ fieldError('campaignId') }}</div>
        </div>

        <div class="form-group">
          <label for="category">Category</label>
          <input id="category" formControlName="category" type="text" maxlength="100" />
          <div *ngIf="fieldError('category')" class="field-error">{{ fieldError('category') }}</div>
        </div>

        <div class="form-group">
          <label for="priority">Priority</label>
          <select id="priority" formControlName="priority">
            <option value="">Select a priority</option>
            <option value="Low">Low</option>
            <option value="Normal">Normal</option>
            <option value="High">High</option>
            <option value="Critical">Critical</option>
          </select>
          <div *ngIf="fieldError('priority')" class="field-error">{{ fieldError('priority') }}</div>
        </div>

        <div class="form-group">
          <label for="subject">Subject</label>
          <input id="subject" formControlName="subject" type="text" maxlength="200" />
          <div *ngIf="fieldError('subject')" class="field-error">{{ fieldError('subject') }}</div>
        </div>

        <div class="form-group">
          <label for="description">Description</label>
          <textarea id="description" formControlName="description" rows="5"></textarea>
          <div *ngIf="fieldError('description')" class="field-error">{{ fieldError('description') }}</div>
        </div>

        <div *ngIf="generalError" class="error">{{ generalError }}</div>

        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="submitting">
            {{ submitting ? 'Saving...' : 'Create Ticket' }}
          </button>
          <a routerLink="/tickets" class="btn btn-secondary">Cancel</a>
        </div>
      </form>
    </div>
  `
})
export class TicketCreateComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly ticketService = inject(TicketService);
  private readonly customerService = inject(CustomerService);
  private readonly organizationService = inject(OrganizationService);
  private readonly errorParser = inject(ApiErrorParserService);

  customers: Customer[] = [];
  accounts: Account[] = [];
  campaigns: Campaign[] = [];
  submitting = false;
  generalError: string | null = null;
  fieldErrors: Record<string, string> = {};

  form = this.fb.group({
    customerId: ['', Validators.required],
    accountId: ['', Validators.required],
    campaignId: ['', Validators.required],
    category: ['', [Validators.required, Validators.maxLength(100)]],
    priority: ['', Validators.required],
    subject: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', Validators.required]
  });

  fieldError(name: string): string | null {
    return this.fieldErrors[name] ?? null;
  }

  ngOnInit() {
    this.customerService.getCustomers().subscribe((customers) => {
      this.customers = customers.filter((c) => c.isActive);
    });

    this.organizationService.getAccounts().subscribe((accounts) => {
      this.accounts = accounts.filter((a) => a.isActive);
    });

    this.organizationService.getCampaigns().subscribe((campaigns) => {
      this.campaigns = campaigns.filter((c) => c.isActive);
    });
  }

  submit() {
    if (this.form.invalid || this.submitting) return;
    this.submitting = true;
    this.generalError = null;
    this.fieldErrors = {};

    const value = this.form.getRawValue();
    const request = {
      customerId: value.customerId!,
      accountId: value.accountId!,
      campaignId: value.campaignId!,
      category: value.category!,
      priority: value.priority!,
      subject: value.subject!,
      description: value.description!
    };

    this.ticketService.createTicket(request).subscribe({
      next: (ticket) => this.router.navigate(['/tickets', ticket.id]),
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
