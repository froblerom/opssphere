import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { TicketListComponent } from './ticket-list.component';
import { TicketService } from './ticket.service';

describe('TicketListComponent', () => {
  let ticketService: jasmine.SpyObj<Pick<TicketService, 'getTickets'>>;
  let authService: jasmine.SpyObj<Pick<AuthService, 'hasPermission'>>;
  let fixture: ComponentFixture<TicketListComponent>;

  beforeEach(() => {
    ticketService = jasmine.createSpyObj<Pick<TicketService, 'getTickets'>>('TicketService', ['getTickets']);
    authService = jasmine.createSpyObj<Pick<AuthService, 'hasPermission'>>('AuthService', ['hasPermission']);
    ticketService.getTickets.and.returnValue(of([]));
    authService.hasPermission.and.returnValue(false);

    TestBed.configureTestingModule({
      imports: [TicketListComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({
                status: 'Open',
                priority: 'High',
                slaState: 'AtRisk',
                regionId: 'region-1',
                countryId: 'country-1',
                accountId: 'account-1',
                campaignId: 'campaign-1',
                supervisorUserId: 'supervisor-1',
                assignedAgentUserId: 'agent-1',
                isEscalated: 'true',
                dateFrom: '2026-05-01T00:00:00Z',
                dateTo: '2026-05-18T00:00:00Z'
              })
            }
          }
        },
        { provide: TicketService, useValue: ticketService },
        { provide: AuthService, useValue: authService }
      ]
    });

    fixture = TestBed.createComponent(TicketListComponent);
    fixture.detectChanges();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('reads query params on init and applies them to ticket filters', () => {
    expect(ticketService.getTickets).toHaveBeenCalledOnceWith(jasmine.objectContaining({
      status: 'Open',
      priority: 'High',
      slaState: 'AtRisk',
      regionId: 'region-1',
      countryId: 'country-1',
      accountId: 'account-1',
      campaignId: 'campaign-1',
      supervisorUserId: 'supervisor-1',
      assignedAgentUserId: 'agent-1',
      isEscalated: 'true',
      dateFrom: '2026-05-01T00:00:00Z',
      dateTo: '2026-05-18T00:00:00Z'
    }));
  });
});
