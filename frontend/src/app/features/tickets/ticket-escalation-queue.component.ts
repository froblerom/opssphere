import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { SafeApiError } from '../../core/models/api-error.models';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { EscalationQueueItemDto } from './ticket.models';
import { TicketService } from './ticket.service';

@Component({
  selector: 'app-ticket-escalation-queue',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Escalations</h1>
      </div>

      <div *ngIf="loading" class="loading">Loading escalations...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <table *ngIf="!loading && escalations.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Ticket #</th>
            <th>Customer</th>
            <th>Account</th>
            <th>Campaign</th>
            <th>Priority</th>
            <th>Status</th>
            <th>SLA State</th>
            <th>Escalated By</th>
            <th>Escalated At</th>
            <th>Reason</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let escalation of escalations">
            <td>
              <a [routerLink]="['/tickets', escalation.ticketId]">{{ escalation.ticketNumber }}</a>
            </td>
            <td>{{ escalation.customerName }}</td>
            <td>{{ escalation.accountName }}</td>
            <td>{{ escalation.campaignName }}</td>
            <td>{{ escalation.priority }}</td>
            <td>{{ escalation.status }}</td>
            <td>{{ escalation.slaState }}</td>
            <td>{{ escalation.escalatedByName }}</td>
            <td>{{ escalation.escalatedAt | date:'medium' }}</td>
            <td>{{ escalation.escalationReason }}</td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && escalations.length === 0 && !error">No active escalations found.</p>
    </div>
  `
})
export class TicketEscalationQueueComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  private readonly errorParser = inject(ApiErrorParserService);

  escalations: EscalationQueueItemDto[] = [];
  loading = true;
  error: string | null = null;

  ngOnInit() {
    this.ticketService.getEscalationQueue().subscribe({
      next: (escalations) => {
        this.escalations = escalations;
        this.loading = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.error = parsed.message;
        this.escalations = [];
        this.loading = false;
      }
    });
  }
}
