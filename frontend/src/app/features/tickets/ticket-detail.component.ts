import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

import { TicketService } from './ticket.service';
import { TicketDetail } from './ticket.models';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div *ngIf="loading" class="loading">Loading...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <ng-container *ngIf="ticket">
        <div class="page-header">
          <h1>Ticket {{ ticket.ticketNumber }}</h1>
        </div>

        <dl class="detail-list">
          <dt>Ticket Number</dt>
          <dd>{{ ticket.ticketNumber }}</dd>
          <dt>Status</dt>
          <dd>{{ ticket.status }}</dd>
          <dt>Priority</dt>
          <dd>{{ ticket.priority }}</dd>
          <dt>SLA State</dt>
          <dd>{{ ticket.slaState }}</dd>
          <dt>Customer</dt>
          <dd>{{ ticket.customerName }}</dd>
          <dt>Account</dt>
          <dd>{{ ticket.accountName }}</dd>
          <dt>Campaign</dt>
          <dd>{{ ticket.campaignName }}</dd>
          <dt>Category</dt>
          <dd>{{ ticket.category }}</dd>
          <dt>Subject</dt>
          <dd>{{ ticket.subject }}</dd>
          <dt>Description</dt>
          <dd>{{ ticket.description }}</dd>
          <dt>SLA Due</dt>
          <dd>{{ ticket.slaDueAt ? (ticket.slaDueAt | date:'medium') : '—' }}</dd>
          <dt>Created</dt>
          <dd>{{ ticket.createdAt | date:'medium' }}</dd>
          <dt>Updated</dt>
          <dd>{{ ticket.updatedAt ? (ticket.updatedAt | date:'medium') : '—' }}</dd>
        </dl>
      </ng-container>

      <a routerLink="/tickets">Back to list</a>
    </div>
  `
})
export class TicketDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly ticketService = inject(TicketService);

  ticket: TicketDetail | null = null;
  loading = true;
  error: string | null = null;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.ticketService.getTicket(id).subscribe({
      next: (ticket) => {
        this.ticket = ticket;
        this.loading = false;
      },
      error: () => {
        this.error = 'Ticket not found.';
        this.loading = false;
      }
    });
  }
}
