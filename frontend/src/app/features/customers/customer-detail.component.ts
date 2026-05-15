import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { CustomerService } from './customer.service';
import { Customer } from './customer.models';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div *ngIf="loading" class="loading">Loading...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <ng-container *ngIf="customer">
        <div class="page-header">
          <h1>{{ customer.firstName }} {{ customer.lastName }}</h1>
          <div class="actions">
            <a *ngIf="canUpdate" [routerLink]="['/customers', customer.id, 'edit']" class="btn btn-secondary">Edit</a>
            <button *ngIf="canUpdate && customer.isActive" (click)="deactivate()" class="btn btn-danger">Deactivate</button>
          </div>
        </div>

        <dl class="detail-list">
          <dt>Account</dt>
          <dd>{{ customer.accountName }} ({{ customer.accountCode }})</dd>
          <dt>Email</dt>
          <dd>{{ customer.email ?? '—' }}</dd>
          <dt>Phone</dt>
          <dd>{{ customer.phoneNumber ?? '—' }}</dd>
          <dt>External Reference</dt>
          <dd>{{ customer.externalReference ?? '—' }}</dd>
          <dt>Status</dt>
          <dd>
            <span [class]="customer.isActive ? 'badge badge-active' : 'badge badge-inactive'">
              {{ customer.isActive ? 'Active' : 'Inactive' }}
            </span>
          </dd>
          <dt>Created</dt>
          <dd>{{ customer.createdAt | date:'medium' }}</dd>
          <dt>Updated</dt>
          <dd>{{ customer.updatedAt ? (customer.updatedAt | date:'medium') : '—' }}</dd>
        </dl>

        <div *ngIf="canViewHistory" class="ticket-history">
          <h2>Ticket History</h2>
          <p class="placeholder-note">Ticket history will be available in a future release.</p>
        </div>
      </ng-container>

      <a routerLink="/customers">Back to list</a>
    </div>
  `
})
export class CustomerDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly customerService = inject(CustomerService);
  private readonly authService = inject(AuthService);

  customer: Customer | null = null;
  loading = true;
  error: string | null = null;

  get canUpdate() {
    return this.authService.hasPermission(AppPermissions.CustomersUpdate);
  }

  get canViewHistory() {
    return this.authService.hasPermission(AppPermissions.CustomersHistoryView);
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.customerService.getCustomer(id).subscribe({
      next: (customer) => {
        this.customer = customer;
        this.loading = false;
      },
      error: () => {
        this.error = 'Customer not found.';
        this.loading = false;
      }
    });
  }

  deactivate() {
    if (!this.customer) return;
    this.customerService.deactivateCustomer(this.customer.id).subscribe({
      next: () => this.router.navigate(['/customers']),
      error: () => {
        this.error = 'Failed to deactivate customer.';
      }
    });
  }
}
