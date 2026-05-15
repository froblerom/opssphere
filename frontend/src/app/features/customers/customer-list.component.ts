import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { CustomerService } from './customer.service';
import { Customer } from './customer.models';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Customers</h1>
        <a *ngIf="canCreate" routerLink="/customers/create" class="btn btn-primary">New Customer</a>
      </div>

      <div *ngIf="loading" class="loading">Loading customers...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <table *ngIf="!loading && customers.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Account</th>
            <th>Email</th>
            <th>External Reference</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let customer of customers">
            <td>{{ customer.firstName }} {{ customer.lastName }}</td>
            <td>{{ customer.accountName }}</td>
            <td>{{ customer.email ?? '—' }}</td>
            <td>{{ customer.externalReference ?? '—' }}</td>
            <td>
              <span [class]="customer.isActive ? 'badge badge-active' : 'badge badge-inactive'">
                {{ customer.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td>
              <a [routerLink]="['/customers', customer.id]">View</a>
              <a *ngIf="canUpdate" [routerLink]="['/customers', customer.id, 'edit']">Edit</a>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && customers.length === 0">No customers found in your scope.</p>
    </div>
  `
})
export class CustomerListComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly authService = inject(AuthService);

  customers: Customer[] = [];
  loading = true;
  error: string | null = null;

  get canCreate() {
    return this.authService.hasPermission(AppPermissions.CustomersCreate);
  }

  get canUpdate() {
    return this.authService.hasPermission(AppPermissions.CustomersUpdate);
  }

  ngOnInit() {
    this.customerService.getCustomers().subscribe({
      next: (customers) => {
        this.customers = customers;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load customers.';
        this.loading = false;
      }
    });
  }
}
