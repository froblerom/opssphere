import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { SlaStateBadgeComponent } from '../../shared/components/sla-state-badge.component';
import { TicketService } from './ticket.service';
import { TicketListFilter, TicketListItem } from './ticket.models';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SlaStateBadgeComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Tickets</h1>
        <a *ngIf="canCreate" routerLink="/tickets/create" class="btn btn-primary">New Ticket</a>
      </div>

      <form class="ticket-filters" (ngSubmit)="loadTickets()">
        <label>
          Status
          <select name="status" [(ngModel)]="filters.status" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let status of statusOptions" [value]="status">{{ status }}</option>
          </select>
        </label>

        <label>
          Priority
          <select name="priority" [(ngModel)]="filters.priority" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let priority of priorityOptions" [value]="priority">{{ priority }}</option>
          </select>
        </label>

        <label>
          SLA
          <select name="slaState" [(ngModel)]="filters.slaState" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let state of slaStateOptions" [value]="state">{{ slaStateLabel(state) }}</option>
          </select>
        </label>

        <button type="button" class="btn btn-secondary" (click)="clearFilters()">Clear</button>
      </form>

      <div *ngIf="loading" class="loading">Loading tickets...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <table *ngIf="!loading && tickets.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Ticket #</th>
            <th>Customer</th>
            <th>Campaign</th>
            <th>Priority</th>
            <th>Status</th>
            <th>SLA State</th>
            <th>Created</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let ticket of tickets">
            <td>{{ ticket.ticketNumber }}</td>
            <td>{{ ticket.customerName }}</td>
            <td>{{ ticket.campaignName }}</td>
            <td>{{ ticket.priority }}</td>
            <td>{{ ticket.status }}</td>
            <td><app-sla-state-badge [slaState]="ticket.slaState" /></td>
            <td>{{ ticket.createdAt | date:'medium' }}</td>
            <td>
              <a [routerLink]="['/tickets', ticket.id]">View</a>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && tickets.length === 0">No tickets found.</p>
    </div>
  `,
  styles: [`
    .ticket-filters {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
      align-items: end;
      margin: 0 0 16px;
    }

    .ticket-filters label {
      display: grid;
      gap: 4px;
      font-size: 0.9rem;
      font-weight: 600;
    }

    .ticket-filters select {
      min-width: 150px;
    }

  `]
})
export class TicketListComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);

  tickets: TicketListItem[] = [];
  readonly statusOptions = ['Open', 'Assigned', 'InProgress', 'WaitingForCustomer', 'Escalated', 'Resolved', 'Closed'];
  readonly priorityOptions = ['Low', 'Normal', 'High', 'Critical'];
  readonly slaStateOptions = ['WithinSla', 'AtRisk', 'Breached', 'Completed'];
  filters: TicketListFilter = {
    status: '',
    priority: '',
    slaState: ''
  };
  loading = true;
  error: string | null = null;

  get canCreate() {
    return this.authService.hasPermission(AppPermissions.TicketsCreate);
  }

  ngOnInit() {
    this.loadTickets();
  }

  loadTickets() {
    this.loading = true;
    this.error = null;

    this.ticketService.getTickets(this.filters).subscribe({
      next: (tickets) => {
        this.tickets = tickets;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load tickets.';
        this.loading = false;
      }
    });
  }

  clearFilters() {
    this.filters = { status: '', priority: '', slaState: '' };
    this.loadTickets();
  }

  slaStateLabel(state: string) {
    return state === 'WithinSla' ? 'Within SLA' : state === 'AtRisk' ? 'At Risk' : state;
  }
}
