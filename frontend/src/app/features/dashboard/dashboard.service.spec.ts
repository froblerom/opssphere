import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { DashboardService } from './dashboard.service';
import { OperationalDashboard } from './dashboard.models';

describe('DashboardService', () => {
  let service: DashboardService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get'>>('ApiClientService', ['get']);

    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        { provide: ApiClientService, useValue: apiClient }
      ]
    });

    service = TestBed.inject(DashboardService);
  });

  it('getOperationalDashboard calls correct endpoint and unwraps data', (done) => {
    const dashboard = buildDashboard();
    apiClient.get.and.returnValue(of({ data: dashboard }));

    service.getOperationalDashboard().subscribe((result) => {
      expect(result).toEqual(dashboard);
      expect(apiClient.get).toHaveBeenCalledOnceWith('dashboard/operational');
      done();
    });
  });

  it('getOperationalDashboard serializes filter query params', (done) => {
    apiClient.get.and.returnValue(of({ data: buildDashboard() }));

    service.getOperationalDashboard({
      regionId: 'region-1',
      countryId: 'country-1',
      accountId: 'account-1',
      campaignId: 'campaign-1',
      supervisorUserId: 'supervisor-1',
      agentUserId: 'agent-1',
      status: 'Open',
      priority: 'High',
      slaState: 'AtRisk',
      dateFrom: '2026-05-01T00:00:00Z',
      dateTo: '2026-05-18T00:00:00Z'
    }).subscribe(() => {
      expect(apiClient.get).toHaveBeenCalledOnceWith('dashboard/operational?regionId=region-1&countryId=country-1&accountId=account-1&campaignId=campaign-1&supervisorUserId=supervisor-1&agentUserId=agent-1&status=Open&priority=High&slaState=AtRisk&dateFrom=2026-05-01T00%3A00%3A00Z&dateTo=2026-05-18T00%3A00%3A00Z');
      done();
    });
  });
});

function buildDashboard(): OperationalDashboard {
  return {
    generatedAtUtc: '2026-05-18T00:00:00Z',
    totalTicketCount: 0,
    openTicketCount: 0,
    assignedTicketCount: 0,
    escalatedTicketCount: 0,
    breachedTicketCount: 0,
    atRiskTicketCount: 0,
    ticketsByStatus: [],
    ticketsByPriority: [],
    ticketsBySlaState: [],
    ticketsByAccount: [],
    ticketsByCampaign: [],
    ticketsByAssignedAgent: [],
    ticketsBySupervisor: [],
    appliedFilters: {}
  };
}
