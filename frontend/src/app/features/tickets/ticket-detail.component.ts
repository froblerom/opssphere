import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { SafeApiError } from '../../core/models/api-error.models';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { TicketService } from './ticket.service';
import { EligibleAgentDto, TicketDetail } from './ticket.models';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
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
          <dt>Assigned Agent</dt>
          <dd>{{ ticket.assignedAgentName || 'Unassigned' }}</dd>
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
          <dd>{{ ticket.slaDueAt ? (ticket.slaDueAt | date:'medium') : 'Not set' }}</dd>
          <dt>Created</dt>
          <dd>{{ ticket.createdAt | date:'medium' }}</dd>
          <dt>Updated</dt>
          <dd>{{ ticket.updatedAt ? (ticket.updatedAt | date:'medium') : 'Not set' }}</dd>
        </dl>

        <section *ngIf="canShowStatusUpdate" class="status-section" aria-labelledby="status-update-title">
          <h2 id="status-update-title">Status Update</h2>
          <p>Current status: {{ ticket.status }}</p>

          <form [formGroup]="statusForm" (ngSubmit)="updateStatus()">
            <div class="form-group">
              <label for="status">Status</label>
              <select id="status" formControlName="status">
                <option value="">Select a status</option>
                <option *ngFor="let status of statusOptions" [value]="status">
                  {{ status }}
                </option>
              </select>
              <div *ngIf="statusFieldError('status')" class="field-error">
                {{ statusFieldError('status') }}
              </div>
            </div>

            <div class="form-group">
              <label for="statusChangeReason">Reason</label>
              <textarea id="statusChangeReason" formControlName="changeReason" rows="3" maxlength="500"></textarea>
              <div *ngIf="statusFieldError('changeReason')" class="field-error">
                {{ statusFieldError('changeReason') }}
              </div>
            </div>

            <div *ngIf="statusRequiresAssignedAgent" class="field-error">
              Assign an agent before setting this ticket to Assigned.
            </div>
            <div *ngIf="statusError" class="error">{{ statusError }}</div>

            <button
              type="submit"
              class="btn btn-primary"
              [disabled]="statusSubmitting || statusForm.invalid || statusRequiresAssignedAgent"
            >
              {{ statusSubmitting ? 'Updating...' : 'Update Status' }}
            </button>
          </form>
        </section>

        <section *ngIf="canShowPriorityUpdate" class="priority-section" aria-labelledby="priority-update-title">
          <h2 id="priority-update-title">Priority Update</h2>
          <p>Current priority: {{ ticket.priority }}</p>

          <form [formGroup]="priorityForm" (ngSubmit)="updatePriority()">
            <div class="form-group">
              <label for="priority">Priority</label>
              <select id="priority" formControlName="priority">
                <option value="">Select a priority</option>
                <option *ngFor="let priority of priorityOptions" [value]="priority">
                  {{ priority }}
                </option>
              </select>
              <div *ngIf="priorityFieldError('priority')" class="field-error">
                {{ priorityFieldError('priority') }}
              </div>
            </div>

            <div class="form-group">
              <label for="priorityChangeReason">Reason</label>
              <textarea id="priorityChangeReason" formControlName="changeReason" rows="3" maxlength="500"></textarea>
              <div *ngIf="priorityFieldError('changeReason')" class="field-error">
                {{ priorityFieldError('changeReason') }}
              </div>
            </div>

            <div *ngIf="priorityError" class="error">{{ priorityError }}</div>

            <button
              type="submit"
              class="btn btn-primary"
              [disabled]="prioritySubmitting || priorityForm.invalid"
            >
              {{ prioritySubmitting ? 'Updating...' : 'Update Priority' }}
            </button>
          </form>
        </section>

        <section *ngIf="canShowAssignment" class="assignment-section" aria-labelledby="assignment-title">
          <h2 id="assignment-title">Assignment</h2>

          <div *ngIf="assignmentLoadError" class="error">{{ assignmentLoadError }}</div>

          <form [formGroup]="assignmentForm" (ngSubmit)="assignTicket()">
            <div class="form-group">
              <label for="targetAgentUserId">Agent</label>
              <select id="targetAgentUserId" formControlName="targetAgentUserId">
                <option value="">Select an agent</option>
                <option *ngFor="let agent of eligibleAgents" [value]="agent.userId">
                  {{ agent.displayName }}
                </option>
              </select>
              <div *ngIf="assignmentFieldError('targetAgentUserId')" class="field-error">
                {{ assignmentFieldError('targetAgentUserId') }}
              </div>
            </div>

            <div class="form-group">
              <label for="reassignmentReason">Reason</label>
              <textarea id="reassignmentReason" formControlName="reassignmentReason" rows="3" maxlength="500"></textarea>
              <div *ngIf="assignmentFieldError('reassignmentReason')" class="field-error">
                {{ assignmentFieldError('reassignmentReason') }}
              </div>
            </div>

            <div *ngIf="assignmentError" class="error">{{ assignmentError }}</div>
            <p *ngIf="!assignmentLoading && eligibleAgents.length === 0 && !assignmentLoadError">No eligible agents available.</p>

            <button
              type="submit"
              class="btn btn-primary"
              [disabled]="assignmentLoading || assignmentSubmitting || assignmentForm.invalid || eligibleAgents.length === 0"
            >
              {{ assignmentSubmitting ? 'Assigning...' : 'Assign Ticket' }}
            </button>
          </form>
        </section>
      </ng-container>

      <a routerLink="/tickets">Back to list</a>
    </div>
  `
})
export class TicketDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);
  private readonly errorParser = inject(ApiErrorParserService);

  readonly statusOptions = ['Assigned', 'InProgress', 'WaitingForCustomer'];
  readonly priorityOptions = ['Low', 'Normal', 'High', 'Critical'];

  ticket: TicketDetail | null = null;
  eligibleAgents: EligibleAgentDto[] = [];
  loading = true;
  assignmentLoading = false;
  assignmentSubmitting = false;
  statusSubmitting = false;
  prioritySubmitting = false;
  error: string | null = null;
  assignmentError: string | null = null;
  assignmentLoadError: string | null = null;
  statusError: string | null = null;
  priorityError: string | null = null;
  assignmentFieldErrors: Record<string, string> = {};
  statusFieldErrors: Record<string, string> = {};
  priorityFieldErrors: Record<string, string> = {};

  assignmentForm = this.fb.group({
    targetAgentUserId: ['', Validators.required],
    reassignmentReason: ['', Validators.maxLength(500)]
  });

  statusForm = this.fb.group({
    status: ['', Validators.required],
    changeReason: ['', Validators.maxLength(500)]
  });

  priorityForm = this.fb.group({
    priority: ['', Validators.required],
    changeReason: ['', Validators.maxLength(500)]
  });

  get canShowAssignment() {
    return this.canAssignTickets && this.ticket?.status !== 'Closed';
  }

  get canShowStatusUpdate() {
    return this.canUpdateTicketStatus && this.ticket?.status !== 'Closed';
  }

  get canShowPriorityUpdate() {
    return this.canUpdateTicketPriority && this.ticket?.status !== 'Closed';
  }

  get statusRequiresAssignedAgent() {
    return this.statusForm.controls.status.value === 'Assigned' && !this.ticket?.assignedAgentUserId;
  }

  private get canAssignTickets() {
    return this.authService.hasPermission(AppPermissions.TicketsAssign);
  }

  private get canUpdateTicketStatus() {
    return this.authService.hasPermission(AppPermissions.TicketsUpdateStatus);
  }

  private get canUpdateTicketPriority() {
    return this.authService.hasPermission(AppPermissions.TicketsUpdatePriority);
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.ticketService.getTicket(id).subscribe({
      next: (ticket) => {
        this.ticket = ticket;
        this.loading = false;
        this.loadEligibleAgentsIfAllowed();
      },
      error: () => {
        this.error = 'Ticket not found.';
        this.loading = false;
      }
    });
  }

  assignmentFieldError(name: string): string | null {
    return this.assignmentFieldErrors[name] ?? null;
  }

  statusFieldError(name: string): string | null {
    return this.statusFieldErrors[name] ?? null;
  }

  priorityFieldError(name: string): string | null {
    return this.priorityFieldErrors[name] ?? null;
  }

  assignTicket() {
    if (!this.ticket || this.assignmentForm.invalid || this.assignmentSubmitting) return;

    this.assignmentSubmitting = true;
    this.assignmentError = null;
    this.assignmentFieldErrors = {};

    const value = this.assignmentForm.getRawValue();
    const targetAgentUserId = value.targetAgentUserId!;
    const reassignmentReason = value.reassignmentReason ?? null;

    this.ticketService.assignTicket(this.ticket.id, targetAgentUserId, reassignmentReason).subscribe({
      next: (response) => {
        const assignedAgent = this.eligibleAgents.find((agent) => agent.userId === response.assignedAgentUserId);
        this.ticket = {
          ...this.ticket!,
          assignedAgentUserId: response.assignedAgentUserId,
          assignedAgentName: assignedAgent?.displayName ?? this.ticket!.assignedAgentName,
          status: response.status
        };
        this.assignmentForm.reset({ targetAgentUserId: '', reassignmentReason: '' });
        this.assignmentSubmitting = false;
        this.loadEligibleAgentsIfAllowed();
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.assignmentFieldErrors = Object.fromEntries(
          parsed.details.filter((d) => d.field).map((d) => [d.field!, d.message])
        );
        this.assignmentError = parsed.details.length === 0 ? parsed.message : null;
        this.assignmentSubmitting = false;
      }
    });
  }

  updateStatus() {
    if (!this.ticket || this.statusForm.invalid || this.statusSubmitting || this.statusRequiresAssignedAgent) return;

    this.statusSubmitting = true;
    this.statusError = null;
    this.statusFieldErrors = {};

    const value = this.statusForm.getRawValue();
    const status = value.status!;
    const changeReason = value.changeReason ?? null;

    this.ticketService.updateTicketStatus(this.ticket.id, status, changeReason).subscribe({
      next: (response) => {
        this.ticket = {
          ...this.ticket!,
          status: response.newStatus
        };
        this.statusForm.reset({ status: '', changeReason: '' });
        this.statusSubmitting = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.statusFieldErrors = Object.fromEntries(
          parsed.details.filter((d) => d.field).map((d) => [d.field!, d.message])
        );
        this.statusError = parsed.details.length === 0 ? parsed.message : null;
        this.statusSubmitting = false;
      }
    });
  }

  updatePriority() {
    if (!this.ticket || this.priorityForm.invalid || this.prioritySubmitting) return;

    this.prioritySubmitting = true;
    this.priorityError = null;
    this.priorityFieldErrors = {};

    const value = this.priorityForm.getRawValue();
    const priority = value.priority!;
    const changeReason = value.changeReason ?? null;

    this.ticketService.updateTicketPriority(this.ticket.id, priority, changeReason).subscribe({
      next: (response) => {
        this.ticket = {
          ...this.ticket!,
          priority: response.newPriority
        };
        this.priorityForm.reset({ priority: '', changeReason: '' });
        this.prioritySubmitting = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.priorityFieldErrors = Object.fromEntries(
          parsed.details.filter((d) => d.field).map((d) => [d.field!, d.message])
        );
        this.priorityError = parsed.details.length === 0 ? parsed.message : null;
        this.prioritySubmitting = false;
      }
    });
  }

  private loadEligibleAgentsIfAllowed() {
    if (!this.ticket || !this.canShowAssignment) return;

    this.assignmentLoading = true;
    this.assignmentLoadError = null;

    this.ticketService.getEligibleAgents(this.ticket.id).subscribe({
      next: (agents) => {
        this.eligibleAgents = agents;
        this.assignmentLoading = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.assignmentLoadError = parsed.message;
        this.eligibleAgents = [];
        this.assignmentLoading = false;
      }
    });
  }
}
