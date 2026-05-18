import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { defer, of, throwError } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { AppPermissions } from '../../core/auth/auth-permissions';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { AuditService } from '../audit/audit.service';
import { TicketDetailComponent } from './ticket-detail.component';
import { TicketCommentDto, TicketDetail } from './ticket.models';
import { TicketService } from './ticket.service';

describe('TicketDetailComponent', () => {
  let ticketService: jasmine.SpyObj<Pick<TicketService, 'getTicket' | 'getEligibleAgents' | 'assignTicket' | 'updateTicketStatus' | 'updateTicketPriority' | 'getComments' | 'addComment' | 'escalateTicket' | 'resolveTicket' | 'closeTicket' | 'getTicketHistory'>>;
  let auditService: jasmine.SpyObj<Pick<AuditService, 'getEntityAuditHistory'>>;
  let authService: jasmine.SpyObj<Pick<AuthService, 'hasPermission'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let fixture: ComponentFixture<TicketDetailComponent>;

  beforeEach(() => {
    ticketService = jasmine.createSpyObj<Pick<TicketService, 'getTicket' | 'getEligibleAgents' | 'assignTicket' | 'updateTicketStatus' | 'updateTicketPriority' | 'getComments' | 'addComment' | 'escalateTicket' | 'resolveTicket' | 'closeTicket' | 'getTicketHistory'>>(
      'TicketService',
      ['getTicket', 'getEligibleAgents', 'assignTicket', 'updateTicketStatus', 'updateTicketPriority', 'getComments', 'addComment', 'escalateTicket', 'resolveTicket', 'closeTicket', 'getTicketHistory']
    );
    auditService = jasmine.createSpyObj<Pick<AuditService, 'getEntityAuditHistory'>>('AuditService', ['getEntityAuditHistory']);
    authService = jasmine.createSpyObj<Pick<AuthService, 'hasPermission'>>('AuthService', ['hasPermission']);
    errorParser = jasmine.createSpyObj<Pick<ApiErrorParserService, 'parse'>>('ApiErrorParserService', ['parse']);

    ticketService.getEligibleAgents.and.returnValue(of([
      {
        userId: 'agent-1',
        displayName: 'Nova Agent',
        scopeType: 'Campaign',
        scopeReference: 'NOVABANK-CC'
      }
    ]));
    ticketService.getComments.and.returnValue(of([]));
    ticketService.getTicketHistory.and.returnValue(of([]));
    auditService.getEntityAuditHistory.and.returnValue(of({
      items: [],
      page: 1,
      pageSize: 25,
      totalCount: 0,
      totalPages: 0
    }));
    errorParser.parse.and.returnValue({ code: 'bad_request', message: 'Request failed.', details: [] });

    TestBed.configureTestingModule({
      imports: [TicketDetailComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'ticket-1' })
            }
          }
        },
        { provide: TicketService, useValue: ticketService },
        { provide: AuditService, useValue: auditService },
        { provide: AuthService, useValue: authService },
        { provide: ApiErrorParserService, useValue: errorParser }
      ]
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('shows assignment control for TicketsAssign permission and non-Closed ticket', () => {
    authService.hasPermission.and.returnValue(true);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Assignment');
    expect(text).toContain('Nova Agent');
    expect(ticketService.getEligibleAgents).toHaveBeenCalledOnceWith('ticket-1');
  });

  it('hides assignment control for Closed ticket', () => {
    authService.hasPermission.and.returnValue(true);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Closed' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).not.toContain('Assign Ticket');
    expect(ticketService.getEligibleAgents).not.toHaveBeenCalled();
  });

  it('calls assign service on submit and updates local assignment', () => {
    authService.hasPermission.and.returnValue(true);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.assignTicket.and.returnValue(of({
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      assignedAgentUserId: 'agent-1',
      previousAgentUserId: null,
      status: 'Assigned',
      message: 'Ticket assigned successfully.'
    }));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.assignmentForm.setValue({
      targetAgentUserId: 'agent-1',
      reassignmentReason: 'Coverage'
    });
    component.assignTicket();

    expect(ticketService.assignTicket).toHaveBeenCalledOnceWith('ticket-1', 'agent-1', 'Coverage');
    expect(component.ticket?.assignedAgentUserId).toBe('agent-1');
    expect(component.ticket?.assignedAgentName).toBe('Nova Agent');
    expect(component.ticket?.status).toBe('Assigned');
  });

  it('displays parsed API errors inline', () => {
    authService.hasPermission.and.returnValue(true);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.assignTicket.and.returnValue(throwError(() => new Error('boom')));
    errorParser.parse.and.returnValue({ code: 'agent_inactive', message: 'Agent is inactive.', details: [] });

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.assignmentForm.setValue({
      targetAgentUserId: 'agent-1',
      reassignmentReason: ''
    });
    component.assignTicket();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Agent is inactive.');
  });

  it('shows comments list for users with TicketsView permission', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsView);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.getComments.and.returnValue(of([
      buildComment({ body: 'Checked the billing timeline.' })
    ]));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Internal Comments');
    expect(text).toContain('Nova Agent');
    expect(text).toContain('Checked the billing timeline.');
    expect(ticketService.getComments).toHaveBeenCalledOnceWith('ticket-1');
  });

  it('lazy-loads audit history when AuditView permission is present', async () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.AuditView);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    auditService.getEntityAuditHistory.and.returnValue(defer(() => Promise.resolve({
      items: [
        {
          id: 'audit-1',
          actorUserId: 'user-1',
          actorDisplayName: 'Nova Supervisor',
          action: 'TicketCreated',
          entityType: 'Ticket',
          entityId: 'ticket-1',
          createdAt: '2026-05-18T00:00:00Z',
          correlationId: 'corr-1'
        }
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
      totalPages: 1
    })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(auditService.getEntityAuditHistory).not.toHaveBeenCalled();

    fixture.componentInstance.toggleAuditHistory();
    await fixture.whenStable();

    expect(auditService.getEntityAuditHistory).toHaveBeenCalledOnceWith('Ticket', 'ticket-1');
    expect(fixture.componentInstance.auditHistoryExpanded).toBeTrue();
    expect(fixture.componentInstance.auditHistory).toEqual([
      jasmine.objectContaining({
        action: 'TicketCreated',
        actorDisplayName: 'Nova Supervisor'
      })
    ]);
  });

  it('shows add-comment form for TicketsComment and non-Closed ticket', () => {
    authService.hasPermission.and.callFake((permission) =>
      permission === AppPermissions.TicketsView || permission === AppPermissions.TicketsComment
    );
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Comment');
    expect(text).toContain('Add Comment');
  });

  it('hides add-comment form for Closed ticket', () => {
    authService.hasPermission.and.callFake((permission) =>
      permission === AppPermissions.TicketsView || permission === AppPermissions.TicketsComment
    );
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Closed' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Internal Comments');
    expect(fixture.nativeElement.textContent).not.toContain('Add Comment');
  });

  it('hides add-comment form for users without TicketsComment permission', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsView);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Internal Comments');
    expect(fixture.nativeElement.textContent).not.toContain('Add Comment');
  });

  it('allows viewer without TicketsComment to see comments', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsView);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.getComments.and.returnValue(of([
      buildComment({ body: 'Visible to scoped viewers.' })
    ]));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Visible to scoped viewers.');
    expect(text).not.toContain('Add Comment');
  });

  it('rejects empty comments client-side', () => {
    authService.hasPermission.and.callFake((permission) =>
      permission === AppPermissions.TicketsView || permission === AppPermissions.TicketsComment
    );
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.commentForm.setValue({ body: '   ' });
    component.addComment();
    fixture.detectChanges();

    expect(ticketService.addComment).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Comment body is required.');
  });

  it('submits comment and appends returned comment', () => {
    authService.hasPermission.and.callFake((permission) =>
      permission === AppPermissions.TicketsView || permission === AppPermissions.TicketsComment
    );
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.addComment.and.returnValue(of({
      commentId: 'comment-2',
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      authorUserId: 'agent-1',
      authorDisplayName: 'Nova Agent',
      body: 'Customer ledger reviewed.',
      createdAt: '2026-05-16T00:00:00Z',
      message: 'Comment added successfully.'
    }));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.commentForm.setValue({ body: '  Customer ledger reviewed.  ' });
    component.addComment();

    expect(ticketService.addComment).toHaveBeenCalledOnceWith('ticket-1', 'Customer ledger reviewed.');
    expect(component.comments).toEqual([
      buildComment({
        id: 'comment-2',
        body: 'Customer ledger reviewed.',
        createdAt: '2026-05-16T00:00:00Z'
      })
    ]);
  });

  it('displays comment validation errors inline', () => {
    authService.hasPermission.and.callFake((permission) =>
      permission === AppPermissions.TicketsView || permission === AppPermissions.TicketsComment
    );
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.addComment.and.returnValue(throwError(() => new Error('boom')));
    errorParser.parse.and.returnValue({
      code: 'validation_failed',
      message: 'Validation failed.',
      details: [{ field: 'body', message: 'Comment body is required.' }]
    });

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.commentForm.setValue({ body: 'A comment' });
    component.addComment();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Comment body is required.');
  });

  it('shows escalation section for TicketsEscalate permission and allowed status', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Escalation');
    expect(text).toContain('Escalate Ticket');
  });

  it('hides escalation section for Closed ticket', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Closed' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Escalate Ticket');
  });

  it('hides escalation section for Escalated ticket', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Escalated', isEscalated: true })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Escalate Ticket');
  });

  it('hides escalation section for Resolved ticket', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Resolved' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Escalate Ticket');
  });

  it('hides escalation section without TicketsEscalate permission', () => {
    authService.hasPermission.and.returnValue(false);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Escalate Ticket');
  });

  it('rejects empty escalation reason client-side', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.escalationForm.setValue({ escalationReason: '   ' });
    component.escalateTicket();
    fixture.detectChanges();

    expect(ticketService.escalateTicket).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Escalation reason is required.');
  });

  it('submits escalation with trimmed reason', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.escalateTicket.and.returnValue(of({
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      escalationId: 'escalation-1',
      previousStatus: 'Open',
      newStatus: 'Escalated',
      message: 'Ticket escalated successfully.'
    }));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.escalationForm.setValue({ escalationReason: '  Customer impact requires supervisor review.  ' });
    component.escalateTicket();

    expect(ticketService.escalateTicket).toHaveBeenCalledOnceWith('ticket-1', 'Customer impact requires supervisor review.');
  });

  it('updates ticket status and escalation flag after successful escalation', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open', isEscalated: false })));
    ticketService.escalateTicket.and.returnValue(of({
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      escalationId: 'escalation-1',
      previousStatus: 'Open',
      newStatus: 'Escalated',
      message: 'Ticket escalated successfully.'
    }));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.escalationForm.setValue({ escalationReason: 'Customer impact requires supervisor review.' });
    component.escalateTicket();

    expect(component.ticket?.status).toBe('Escalated');
    expect(component.ticket?.isEscalated).toBeTrue();
    expect(component.canShowEscalation).toBeFalse();
  });

  it('displays escalation validation errors inline', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.escalateTicket.and.returnValue(throwError(() => new Error('boom')));
    errorParser.parse.and.returnValue({
      code: 'validation_failed',
      message: 'Validation failed.',
      details: [{ field: 'escalationReason', message: 'Escalation reason is required.' }]
    });

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.escalationForm.setValue({ escalationReason: 'Needs review.' });
    component.escalateTicket();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Escalation reason is required.');
  });

  it('displays escalation business rule errors inline', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsEscalate);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open' })));
    ticketService.escalateTicket.and.returnValue(throwError(() => new Error('boom')));
    errorParser.parse.and.returnValue({
      code: 'business_rule_violation',
      message: 'Ticket is already escalated.',
      details: []
    });

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.escalationForm.setValue({ escalationReason: 'Needs review.' });
    component.escalateTicket();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Ticket is already escalated.');
  });

  it('shows status controls with TicketsUpdateStatus permission and non-Closed ticket', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdateStatus);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned', assignedAgentUserId: 'agent-1' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Status Update');
    expect(text).toContain('Current status: Assigned');
    expect(text).toContain('InProgress');
    expect(text).not.toContain('Escalated');
    expect(text).not.toContain('Resolved');
    expect(text).not.toContain('Closed');
  });

  it('hides status controls when ticket is Closed', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdateStatus);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Closed' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Status Update');
  });

  it('hides status controls without TicketsUpdateStatus permission', () => {
    authService.hasPermission.and.returnValue(false);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Status Update');
  });

  it('shows priority controls with TicketsUpdatePriority permission and non-Closed ticket', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdatePriority);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned', priority: 'Normal' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Priority Update');
    expect(text).toContain('Current priority: Normal');
    expect(text).toContain('Low');
    expect(text).toContain('Critical');
  });

  it('hides priority controls when ticket is Closed', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdatePriority);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Closed' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Priority Update');
  });

  it('hides priority controls without TicketsUpdatePriority permission', () => {
    authService.hasPermission.and.returnValue(false);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned' })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Priority Update');
  });

  it('calls update status service on submit and updates local status', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdateStatus);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned', assignedAgentUserId: 'agent-1' })));
    ticketService.updateTicketStatus.and.returnValue(of({
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      previousStatus: 'Assigned',
      newStatus: 'InProgress',
      message: 'Ticket status updated successfully.'
    }));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.statusForm.setValue({
      status: 'InProgress',
      changeReason: 'Working now'
    });
    component.updateStatus();

    expect(ticketService.updateTicketStatus).toHaveBeenCalledOnceWith('ticket-1', 'InProgress', 'Working now');
    expect(component.ticket?.status).toBe('InProgress');
  });

  it('prevents setting Assigned when no agent is assigned', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdateStatus);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Open', assignedAgentUserId: null })));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.statusForm.setValue({
      status: 'Assigned',
      changeReason: ''
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Assign an agent before setting this ticket to Assigned.');
    component.updateStatus();
    expect(ticketService.updateTicketStatus).not.toHaveBeenCalled();
  });

  it('calls update priority service on submit and updates local priority', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdatePriority);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned', priority: 'Normal' })));
    ticketService.updateTicketPriority.and.returnValue(of({
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      previousPriority: 'Normal',
      newPriority: 'High',
      message: 'Ticket priority updated successfully.'
    }));

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.priorityForm.setValue({
      priority: 'High',
      changeReason: 'Customer impact'
    });
    component.updatePriority();

    expect(ticketService.updateTicketPriority).toHaveBeenCalledOnceWith('ticket-1', 'High', 'Customer impact');
    expect(component.ticket?.priority).toBe('High');
  });

  it('displays parsed status business rule errors inline', () => {
    authService.hasPermission.and.callFake((permission) => permission === AppPermissions.TicketsUpdateStatus);
    ticketService.getTicket.and.returnValue(of(buildTicket({ status: 'Assigned', assignedAgentUserId: 'agent-1' })));
    ticketService.updateTicketStatus.and.returnValue(throwError(() => new Error('boom')));
    errorParser.parse.and.returnValue({ code: 'business_rule_violation', message: 'Invalid ticket transition.', details: [] });

    fixture = TestBed.createComponent(TicketDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.statusForm.setValue({
      status: 'WaitingForCustomer',
      changeReason: ''
    });
    component.updateStatus();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Invalid ticket transition.');
  });
});

function buildTicket(overrides: Partial<TicketDetail> = {}): TicketDetail {
  return {
    id: 'ticket-1',
    ticketNumber: 'TKT-0001',
    customerName: 'Jane Customer',
    accountName: 'NovaBank',
    campaignName: 'NovaBank Care',
    priority: 'Normal',
    status: 'Open',
    slaState: 'OnTrack',
    isEscalated: false,
    createdAt: '2026-05-15T00:00:00Z',
    assignedAgentUserId: null,
    assignedAgentName: null,
    customerId: 'customer-1',
    accountId: 'account-1',
    campaignId: 'campaign-1',
    category: 'Billing',
    subject: 'Invoice issue',
    description: 'My invoice is wrong.',
    slaDueAt: null,
    createdByUserId: 'user-1',
    updatedAt: null,
    ...overrides
  };
}

function buildComment(overrides: Partial<TicketCommentDto> = {}): TicketCommentDto {
  return {
    id: 'comment-1',
    ticketId: 'ticket-1',
    authorUserId: 'agent-1',
    authorDisplayName: 'Nova Agent',
    body: 'Checked the billing timeline.',
    createdAt: '2026-05-15T01:00:00Z',
    ...overrides
  };
}
