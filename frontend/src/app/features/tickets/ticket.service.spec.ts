import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { TicketService } from './ticket.service';
import { CreateTicketRequest, TicketListItem } from './ticket.models';

describe('TicketService', () => {
  let service: TicketService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get' | 'post'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get' | 'post'>>('ApiClientService', ['get', 'post']);

    TestBed.configureTestingModule({
      providers: [
        TicketService,
        { provide: ApiClientService, useValue: apiClient }
      ]
    });

    service = TestBed.inject(TicketService);
  });

  it('getTickets calls correct URL and unwraps data', (done) => {
    const tickets: TicketListItem[] = [
      {
        id: 'ticket-1',
        ticketNumber: 'TKT-0001',
        customerName: 'John Doe',
        accountName: 'Acme',
        campaignName: 'Support 2026',
        priority: 'Normal',
        status: 'Open',
        slaState: 'OnTrack',
        isEscalated: false,
        createdAt: '2026-05-15T00:00:00Z'
      }
    ];
    apiClient.get.and.returnValue(of({ data: tickets }));

    service.getTickets().subscribe((result) => {
      expect(result).toEqual(tickets);
      expect(apiClient.get).toHaveBeenCalledOnceWith('tickets');
      done();
    });
  });

  it('getTicket calls correct URL and unwraps data', (done) => {
    const ticket = {
      id: 'ticket-1',
      ticketNumber: 'TKT-0001',
      customerName: 'John Doe',
      accountName: 'Acme',
      campaignName: 'Support 2026',
      priority: 'Normal',
      status: 'Open',
      slaState: 'OnTrack',
      isEscalated: false,
      createdAt: '2026-05-15T00:00:00Z',
      customerId: 'customer-1',
      accountId: 'account-1',
      campaignId: 'campaign-1',
      category: 'Billing',
      subject: 'Invoice issue',
      description: 'My invoice is wrong.',
      slaDueAt: null,
      createdByUserId: 'user-1',
      updatedAt: null
    };
    apiClient.get.and.returnValue(of({ data: ticket }));

    service.getTicket('ticket-1').subscribe((result) => {
      expect(result).toEqual(ticket);
      expect(apiClient.get).toHaveBeenCalledOnceWith('tickets/ticket-1');
      done();
    });
  });

  it('createTicket calls correct URL with request and unwraps response', (done) => {
    const request: CreateTicketRequest = {
      customerId: 'customer-1',
      accountId: 'account-1',
      campaignId: 'campaign-1',
      category: 'Billing',
      priority: 'Normal',
      subject: 'Invoice issue',
      description: 'My invoice is wrong.'
    };
    const response = {
      id: 'ticket-1',
      ticketNumber: 'TKT-0001',
      status: 'Open',
      priority: 'Normal',
      slaState: 'OnTrack',
      slaDueAt: null
    };
    apiClient.post.and.returnValue(of({ data: response }));

    service.createTicket(request).subscribe((result) => {
      expect(result).toEqual(response);
      expect(apiClient.post).toHaveBeenCalledOnceWith('tickets', request);
      done();
    });
  });
});
