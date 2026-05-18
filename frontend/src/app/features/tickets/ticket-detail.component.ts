import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { SafeApiError } from '../../core/models/api-error.models';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { SlaStateBadgeComponent } from '../../shared/components/sla-state-badge.component';
import { TicketService } from './ticket.service';
import { EligibleAgentDto, TicketCommentDto, TicketDetail, TicketStatusHistoryItemDto } from './ticket.models';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SlaStateBadgeComponent],
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
          <dd><app-sla-state-badge [slaState]="ticket.slaState" /></dd>
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

        <section *ngIf="canShowEscalation" class="escalation-section" aria-labelledby="escalation-title">
          <h2 id="escalation-title">Escalation</h2>

          <form [formGroup]="escalationForm" (ngSubmit)="escalateTicket()">
            <div class="form-group">
              <label for="escalationReason">Reason</label>
              <textarea id="escalationReason" formControlName="escalationReason" rows="4" maxlength="1000" required></textarea>
              <div *ngIf="escalationFieldError('escalationReason')" class="field-error">
                {{ escalationFieldError('escalationReason') }}
              </div>
            </div>

            <div *ngIf="escalationError" class="error">{{ escalationError }}</div>

            <button
              type="submit"
              class="btn btn-primary"
              [disabled]="escalationSubmitting"
            >
              {{ escalationSubmitting ? 'Escalating...' : 'Escalate Ticket' }}
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

        <section *ngIf="canShowResolve" class="resolve-section" aria-labelledby="resolve-title">
          <h2 id="resolve-title">Resolve Ticket</h2>

          <form [formGroup]="resolveForm" (ngSubmit)="resolveTicket()">
            <div class="form-group">
              <label for="resolutionSummary">Resolution Summary</label>
              <textarea id="resolutionSummary" formControlName="resolutionSummary" rows="4" maxlength="2000" required></textarea>
              <div *ngIf="resolveFieldError('resolutionSummary')" class="field-error">
                {{ resolveFieldError('resolutionSummary') }}
              </div>
            </div>

            <div class="form-group">
              <label for="resolutionCode">Resolution Code (optional)</label>
              <input type="text" id="resolutionCode" formControlName="resolutionCode" maxlength="100" />
              <div *ngIf="resolveFieldError('resolutionCode')" class="field-error">
                {{ resolveFieldError('resolutionCode') }}
              </div>
            </div>

            <div *ngIf="resolveError" class="error">{{ resolveError }}</div>

            <button
              type="submit"
              class="btn btn-primary"
              [disabled]="resolveSubmitting || resolveForm.invalid"
            >
              {{ resolveSubmitting ? 'Resolving...' : 'Resolve Ticket' }}
            </button>
          </form>
        </section>

        <section *ngIf="canShowClose" class="close-section" aria-labelledby="close-title">
          <h2 id="close-title">Close Ticket</h2>

          <div *ngIf="closeError" class="error">{{ closeError }}</div>

          <button
            class="btn btn-primary"
            (click)="closeTicket()"
            [disabled]="closeSubmitting"
          >
            {{ closeSubmitting ? 'Closing...' : 'Close Ticket' }}
          </button>
        </section>

        <section *ngIf="canShowHistory" class="history-section" aria-labelledby="history-title">
          <h2 id="history-title">Status History</h2>

          <div *ngIf="historyError" class="error">{{ historyError }}</div>

          <div *ngIf="statusHistory.length > 0; else noHistory" class="history-list">
            <article *ngFor="let item of statusHistory" class="history-item">
              <div>
                <strong>{{ item.previousStatus ?? '—' }} → {{ item.newStatus }}</strong>
              </div>
              <div *ngIf="item.changeReason">Reason: {{ item.changeReason }}</div>
              <div>{{ item.createdAt | date:'medium' }}</div>
            </article>
          </div>

          <ng-template #noHistory>
            <p>No status history yet.</p>
          </ng-template>
        </section>

        <section *ngIf="canShowComments" class="comments-section" aria-labelledby="comments-title">
          <h2 id="comments-title">Internal Comments</h2>

          <div *ngIf="commentError" class="error">{{ commentError }}</div>

          <div *ngIf="comments.length > 0; else noComments" class="comments-list">
            <article *ngFor="let comment of comments" class="comment-item">
              <header>
                <strong>{{ comment.authorDisplayName }}</strong>
                <span>{{ comment.createdAt | date:'medium' }}</span>
              </header>
              <p>{{ comment.body }}</p>
            </article>
          </div>

          <ng-template #noComments>
            <p>No internal comments yet.</p>
          </ng-template>

          <form *ngIf="canAddComment" [formGroup]="commentForm" (ngSubmit)="addComment()">
            <div class="form-group">
              <label for="commentBody">Comment</label>
              <textarea id="commentBody" formControlName="body" rows="4" maxlength="5000" required></textarea>
              <div *ngIf="commentFieldError('body')" class="field-error">
                {{ commentFieldError('body') }}
              </div>
            </div>

            <button
              type="submit"
              class="btn btn-primary"
              [disabled]="commentSubmitting"
            >
              {{ commentSubmitting ? 'Adding...' : 'Add Comment' }}
            </button>
          </form>
        </section>
      </ng-container>

      <a routerLink="/tickets">Back to list</a>
    </div>
  `,
  styles: []
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
  comments: TicketCommentDto[] = [];
  statusHistory: TicketStatusHistoryItemDto[] = [];
  loading = true;
  assignmentLoading = false;
  assignmentSubmitting = false;
  statusSubmitting = false;
  prioritySubmitting = false;
  commentSubmitting = false;
  escalationSubmitting = false;
  resolveSubmitting = false;
  closeSubmitting = false;
  error: string | null = null;
  assignmentError: string | null = null;
  assignmentLoadError: string | null = null;
  statusError: string | null = null;
  priorityError: string | null = null;
  commentError: string | null = null;
  escalationError: string | null = null;
  resolveError: string | null = null;
  closeError: string | null = null;
  historyError: string | null = null;
  assignmentFieldErrors: Record<string, string> = {};
  statusFieldErrors: Record<string, string> = {};
  priorityFieldErrors: Record<string, string> = {};
  commentFieldErrors: Record<string, string> = {};
  escalationFieldErrors: Record<string, string> = {};
  resolveFieldErrors: Record<string, string> = {};

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

  commentForm = this.fb.group({
    body: ['', [Validators.required, Validators.maxLength(5000)]]
  });

  escalationForm = this.fb.group({
    escalationReason: ['', [Validators.required, Validators.maxLength(1000)]]
  });

  resolveForm = this.fb.group({
    resolutionSummary: ['', [Validators.required, Validators.maxLength(2000)]],
    resolutionCode: ['', Validators.maxLength(100)]
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

  get canShowEscalation() {
    return this.authService.hasPermission(AppPermissions.TicketsEscalate)
      && this.ticket?.status !== 'Closed'
      && this.ticket?.status !== 'Escalated'
      && this.ticket?.status !== 'Resolved';
  }

  get canShowResolve() {
    return this.authService.hasPermission(AppPermissions.TicketsResolve)
      && !['Resolved', 'Closed'].includes(this.ticket?.status ?? '');
  }

  get canShowClose() {
    return this.authService.hasPermission(AppPermissions.TicketsClose)
      && this.ticket?.status === 'Resolved';
  }

  get canShowHistory() {
    return this.authService.hasPermission(AppPermissions.TicketsHistoryView);
  }

  get canShowComments() {
    return this.authService.hasPermission(AppPermissions.TicketsView);
  }

  get canAddComment() {
    return this.authService.hasPermission(AppPermissions.TicketsComment) && this.ticket?.status !== 'Closed';
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
        this.loadCommentsIfAllowed();
        this.loadHistoryIfAllowed();
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

  commentFieldError(name: string): string | null {
    return this.commentFieldErrors[name] ?? null;
  }

  escalationFieldError(name: string): string | null {
    return this.escalationFieldErrors[name] ?? null;
  }

  resolveFieldError(name: string): string | null {
    return this.resolveFieldErrors[name] ?? null;
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

  escalateTicket() {
    if (!this.ticket || this.escalationSubmitting) return;

    this.escalationError = null;
    this.escalationFieldErrors = {};

    const escalationReason = (this.escalationForm.controls.escalationReason.value ?? '').trim();
    if (!escalationReason) {
      this.escalationFieldErrors = { escalationReason: 'Escalation reason is required.' };
      return;
    }

    if (escalationReason.length > 1000) {
      this.escalationFieldErrors = { escalationReason: 'Escalation reason must be 1000 characters or fewer.' };
      return;
    }

    this.escalationSubmitting = true;

    this.ticketService.escalateTicket(this.ticket.id, escalationReason).subscribe({
      next: (response) => {
        this.ticket = {
          ...this.ticket!,
          status: response.newStatus,
          isEscalated: true
        };
        this.escalationForm.reset({ escalationReason: '' });
        this.escalationError = null;
        this.escalationFieldErrors = {};
        this.escalationSubmitting = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        const fieldDetails = parsed.details.filter((d) => d.field);
        this.escalationFieldErrors = Object.fromEntries(fieldDetails.map((d) => [d.field!, d.message]));
        this.escalationError = fieldDetails.length === parsed.details.length && parsed.details.length > 0 ? null : parsed.message;
        this.escalationSubmitting = false;
      }
    });
  }

  addComment() {
    if (!this.ticket || this.commentSubmitting) return;

    this.commentError = null;
    this.commentFieldErrors = {};

    const body = (this.commentForm.controls.body.value ?? '').trim();
    if (!body) {
      this.commentFieldErrors = { body: 'Comment body is required.' };
      return;
    }

    if (body.length > 5000) {
      this.commentFieldErrors = { body: 'Comment body must be 5000 characters or fewer.' };
      return;
    }

    this.commentSubmitting = true;

    this.ticketService.addComment(this.ticket.id, body).subscribe({
      next: (response) => {
        this.comments = [
          ...this.comments,
          {
            id: response.commentId,
            ticketId: response.ticketId,
            authorUserId: response.authorUserId,
            authorDisplayName: response.authorDisplayName,
            body: response.body,
            createdAt: response.createdAt
          }
        ];
        this.commentForm.reset({ body: '' });
        this.commentError = null;
        this.commentFieldErrors = {};
        this.commentSubmitting = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        const fieldDetails = parsed.details.filter((d) => d.field);
        this.commentFieldErrors = Object.fromEntries(fieldDetails.map((d) => [d.field!, d.message]));
        this.commentError = fieldDetails.length === parsed.details.length && parsed.details.length > 0 ? null : parsed.message;
        this.commentSubmitting = false;
      }
    });
  }

  resolveTicket() {
    if (!this.ticket || this.resolveSubmitting) return;

    this.resolveError = null;
    this.resolveFieldErrors = {};

    const summary = (this.resolveForm.controls.resolutionSummary.value ?? '').trim();
    if (!summary) {
      this.resolveFieldErrors = { resolutionSummary: 'Resolution summary is required.' };
      return;
    }

    if (summary.length > 2000) {
      this.resolveFieldErrors = { resolutionSummary: 'Resolution summary must be 2000 characters or fewer.' };
      return;
    }

    const code = (this.resolveForm.controls.resolutionCode.value ?? '').trim() || null;
    if (code && code.length > 100) {
      this.resolveFieldErrors = { resolutionCode: 'Resolution code must be 100 characters or fewer.' };
      return;
    }

    this.resolveSubmitting = true;

    this.ticketService.resolveTicket(this.ticket.id, summary, code).subscribe({
      next: (response) => {
        this.ticket = {
          ...this.ticket!,
          status: response.newStatus,
          resolvedAt: response.resolvedAt
        };
        this.resolveForm.reset({ resolutionSummary: '', resolutionCode: '' });
        this.resolveError = null;
        this.resolveFieldErrors = {};
        this.resolveSubmitting = false;
        this.loadHistoryIfAllowed();
      },
      error: (err) => {
        const parsed = this.errorParser.parse(err);
        const fieldDetails = parsed.details.filter((d) => d.field);
        this.resolveFieldErrors = Object.fromEntries(fieldDetails.map((d) => [d.field!, d.message]));
        this.resolveError = fieldDetails.length === parsed.details.length && parsed.details.length > 0 ? null : parsed.message;
        this.resolveSubmitting = false;
      }
    });
  }

  closeTicket() {
    if (!this.ticket || this.closeSubmitting) return;

    this.closeError = null;
    this.closeSubmitting = true;

    this.ticketService.closeTicket(this.ticket.id).subscribe({
      next: (response) => {
        this.ticket = {
          ...this.ticket!,
          status: response.newStatus,
          closedAt: response.closedAt
        };
        this.closeError = null;
        this.closeSubmitting = false;
        this.loadHistoryIfAllowed();
      },
      error: (err) => {
        const parsed = this.errorParser.parse(err);
        this.closeError = parsed.message;
        this.closeSubmitting = false;
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

  private loadCommentsIfAllowed() {
    if (!this.ticket || !this.canShowComments) return;

    this.ticketService.getComments(this.ticket.id).subscribe({
      next: (comments) => {
        this.comments = comments;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.commentError = parsed.message;
        this.comments = [];
      }
    });
  }

  private loadHistoryIfAllowed() {
    if (!this.ticket || !this.canShowHistory) return;

    this.ticketService.getTicketHistory(this.ticket.id).subscribe({
      next: (history) => {
        this.statusHistory = history;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.historyError = parsed.message;
        this.statusHistory = [];
      }
    });
  }
}
