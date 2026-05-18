import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { TicketService } from './ticket.service';
import { CreateTicketRequest, EligibleAgentDto, TicketCommentDto, TicketListItem } from './ticket.models';

describe('TicketService', () => {
  let service: TicketService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>('ApiClientService', ['get', 'post', 'put']);

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

  it('getEligibleAgents calls correct URL and unwraps data', (done) => {
    const agents: EligibleAgentDto[] = [
      {
        userId: 'agent-1',
        displayName: 'Nova Agent',
        scopeType: 'Campaign',
        scopeReference: 'NOVABANK-CC'
      }
    ];
    apiClient.get.and.returnValue(of({ data: agents }));

    service.getEligibleAgents('ticket-1').subscribe((result) => {
      expect(result).toEqual(agents);
      expect(apiClient.get).toHaveBeenCalledOnceWith('tickets/ticket-1/eligible-agents');
      done();
    });
  });

  it('getComments calls correct URL and unwraps data', (done) => {
    const comments: TicketCommentDto[] = [
      {
        id: 'comment-1',
        ticketId: 'ticket-1',
        authorUserId: 'agent-1',
        authorDisplayName: 'Nova Agent',
        body: 'Checked the billing timeline.',
        createdAt: '2026-05-16T00:00:00Z'
      }
    ];
    apiClient.get.and.returnValue(of({ data: comments }));

    service.getComments('ticket-1').subscribe((result) => {
      expect(result).toEqual(comments);
      expect(apiClient.get).toHaveBeenCalledOnceWith('tickets/ticket-1/comments');
      done();
    });
  });

  it('addComment calls correct URL with request and unwraps response', (done) => {
    const response = {
      commentId: 'comment-1',
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      authorUserId: 'agent-1',
      authorDisplayName: 'Nova Agent',
      body: 'Checked the billing timeline.',
      createdAt: '2026-05-16T00:00:00Z',
      message: 'Comment added successfully.'
    };
    apiClient.post.and.returnValue(of({ data: response }));

    service.addComment('ticket-1', '  Checked the billing timeline.  ').subscribe((result) => {
      expect(result).toEqual(response);
      expect(apiClient.post).toHaveBeenCalledOnceWith('tickets/ticket-1/comments', {
        body: 'Checked the billing timeline.'
      });
      done();
    });
  });

  it('assignTicket calls correct URL with request and unwraps response', (done) => {
    const response = {
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      assignedAgentUserId: 'agent-1',
      previousAgentUserId: null,
      status: 'Assigned',
      message: 'Ticket assigned successfully.'
    };
    apiClient.post.and.returnValue(of({ data: response }));

    service.assignTicket('ticket-1', 'agent-1', '  Escalated workload  ').subscribe((result) => {
      expect(result).toEqual(response);
      expect(apiClient.post).toHaveBeenCalledOnceWith('tickets/ticket-1/assign', {
        targetAgentUserId: 'agent-1',
        reassignmentReason: 'Escalated workload'
      });
      done();
    });
  });

  it('updateTicketStatus calls correct URL with request and unwraps response', (done) => {
    const response = {
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      previousStatus: 'Assigned',
      newStatus: 'InProgress',
      message: 'Ticket status updated successfully.'
    };
    apiClient.put.and.returnValue(of({ data: response }));

    service.updateTicketStatus('ticket-1', 'InProgress', '  Working now  ').subscribe((result) => {
      expect(result).toEqual(response);
      expect(apiClient.put).toHaveBeenCalledOnceWith('tickets/ticket-1/status', {
        status: 'InProgress',
        changeReason: 'Working now'
      });
      done();
    });
  });

  it('updateTicketPriority calls correct URL with request and unwraps response', (done) => {
    const response = {
      ticketId: 'ticket-1',
      ticketNumber: 'TKT-0001',
      previousPriority: 'Normal',
      newPriority: 'High',
      message: 'Ticket priority updated successfully.'
    };
    apiClient.put.and.returnValue(of({ data: response }));

    service.updateTicketPriority('ticket-1', 'High', '  Customer impact  ').subscribe((result) => {
      expect(result).toEqual(response);
      expect(apiClient.put).toHaveBeenCalledOnceWith('tickets/ticket-1/priority', {
        priority: 'High',
        changeReason: 'Customer impact'
      });
      done();
    });
  });
});
