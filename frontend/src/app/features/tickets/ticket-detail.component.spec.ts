import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { TicketDetailComponent } from './ticket-detail.component';
import { TicketDetail } from './ticket.models';
import { TicketService } from './ticket.service';

describe('TicketDetailComponent', () => {
  let ticketService: jasmine.SpyObj<Pick<TicketService, 'getTicket' | 'getEligibleAgents' | 'assignTicket'>>;
  let authService: jasmine.SpyObj<Pick<AuthService, 'hasPermission'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let fixture: ComponentFixture<TicketDetailComponent>;

  beforeEach(() => {
    ticketService = jasmine.createSpyObj<Pick<TicketService, 'getTicket' | 'getEligibleAgents' | 'assignTicket'>>(
      'TicketService',
      ['getTicket', 'getEligibleAgents', 'assignTicket']
    );
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
