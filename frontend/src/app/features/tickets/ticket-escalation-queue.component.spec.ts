import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { EscalationQueueItemDto } from './ticket.models';
import { TicketEscalationQueueComponent } from './ticket-escalation-queue.component';
import { TicketService } from './ticket.service';

describe('TicketEscalationQueueComponent', () => {
  let ticketService: jasmine.SpyObj<Pick<TicketService, 'getEscalationQueue'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let fixture: ComponentFixture<TicketEscalationQueueComponent>;

  beforeEach(() => {
    ticketService = jasmine.createSpyObj<Pick<TicketService, 'getEscalationQueue'>>('TicketService', ['getEscalationQueue']);
    errorParser = jasmine.createSpyObj<Pick<ApiErrorParserService, 'parse'>>('ApiErrorParserService', ['parse']);
    errorParser.parse.and.returnValue({ code: 'bad_request', message: 'Failed to load escalations.', details: [] });

    TestBed.configureTestingModule({
      imports: [TicketEscalationQueueComponent],
      providers: [
        provideRouter([]),
        { provide: TicketService, useValue: ticketService },
        { provide: ApiErrorParserService, useValue: errorParser }
      ]
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('loads queue on init', () => {
    const escalations = [buildEscalation()];
    ticketService.getEscalationQueue.and.returnValue(of(escalations));

    fixture = TestBed.createComponent(TicketEscalationQueueComponent);
    fixture.detectChanges();

    expect(ticketService.getEscalationQueue).toHaveBeenCalledTimes(1);
    expect(fixture.componentInstance.escalations).toEqual(escalations);
  });

  it('renders escalation rows', () => {
    ticketService.getEscalationQueue.and.returnValue(of([buildEscalation()]));

    fixture = TestBed.createComponent(TicketEscalationQueueComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('TKT-0001');
    expect(text).toContain('Jane Customer');
    expect(text).toContain('NovaBank');
    expect(text).toContain('NovaBank Care');
    expect(text).toContain('High');
    expect(text).toContain('Escalated');
    expect(text).toContain('AtRisk');
    expect(text).toContain('Nova Supervisor');
    expect(text).toContain('Customer impact requires supervisor review.');
  });

  it('shows empty state', () => {
    ticketService.getEscalationQueue.and.returnValue(of([]));

    fixture = TestBed.createComponent(TicketEscalationQueueComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No active escalations found.');
  });

  it('links to ticket detail', () => {
    ticketService.getEscalationQueue.and.returnValue(of([buildEscalation()]));

    fixture = TestBed.createComponent(TicketEscalationQueueComponent);
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/tickets/ticket-1');
  });

  it('handles API error', () => {
    ticketService.getEscalationQueue.and.returnValue(throwError(() => new Error('boom')));
    errorParser.parse.and.returnValue({ code: 'server_error', message: 'Unable to load escalation queue.', details: [] });

    fixture = TestBed.createComponent(TicketEscalationQueueComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Unable to load escalation queue.');
    expect(fixture.componentInstance.escalations).toEqual([]);
  });
});

function buildEscalation(overrides: Partial<EscalationQueueItemDto> = {}): EscalationQueueItemDto {
  return {
    escalationId: 'escalation-1',
    ticketId: 'ticket-1',
    ticketNumber: 'TKT-0001',
    customerName: 'Jane Customer',
    accountName: 'NovaBank',
    campaignName: 'NovaBank Care',
    priority: 'High',
    status: 'Escalated',
    slaState: 'AtRisk',
    escalatedAt: '2026-05-18T00:00:00Z',
    escalatedByUserId: 'supervisor-1',
    escalatedByName: 'Nova Supervisor',
    escalationReason: 'Customer impact requires supervisor review.',
    ...overrides
  };
}
