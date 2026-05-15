import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { TicketService } from './ticket.service';
import { TicketListItem } from './ticket.models';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Tickets</h1>
        <a *ngIf="canCreate" routerLink="/tickets/create" class="btn btn-primary">New Ticket</a>
      </div>

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
            <td>{{ ticket.slaState }}</td>
            <td>{{ ticket.createdAt | date:'medium' }}</td>
            <td>
              <a [routerLink]="['/tickets', ticket.id]">View</a>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && tickets.length === 0">No tickets found.</p>
    </div>
  `
})
export class TicketListComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);

  tickets: TicketListItem[] = [];
  loading = true;
  error: string | null = null;

  get canCreate() {
    return this.authService.hasPermission(AppPermissions.TicketsCreate);
  }

  ngOnInit() {
    this.ticketService.getTickets().subscribe({
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
}
