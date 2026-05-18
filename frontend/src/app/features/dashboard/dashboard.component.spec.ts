import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { NEVER, of, throwError } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { DashboardComponent } from './dashboard.component';
import { OperationalDashboard } from './dashboard.models';
import { DashboardService } from './dashboard.service';

describe('DashboardComponent', () => {
  let dashboardService: jasmine.SpyObj<Pick<DashboardService, 'getOperationalDashboard'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let fixture: ComponentFixture<DashboardComponent>;

  beforeEach(() => {
    dashboardService = jasmine.createSpyObj<Pick<DashboardService, 'getOperationalDashboard'>>('DashboardService', ['getOperationalDashboard']);
    errorParser = jasmine.createSpyObj<Pick<ApiErrorParserService, 'parse'>>('ApiErrorParserService', ['parse']);
    errorParser.parse.and.returnValue({ code: 'server_error', message: 'Dashboard failed.', details: [] });

    TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: DashboardService, useValue: dashboardService },
        { provide: ApiErrorParserService, useValue: errorParser }
      ]
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('shows loading state', () => {
    dashboardService.getOperationalDashboard.and.returnValue(NEVER);

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading dashboard...');
  });

  it('shows summary counts', () => {
    dashboardService.getOperationalDashboard.and.returnValue(of(buildDashboard({
      totalTicketCount: 5,
      openTicketCount: 2,
      assignedTicketCount: 3,
      escalatedTicketCount: 1,
      breachedTicketCount: 1,
      atRiskTicketCount: 2
    })));

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Total tickets');
    expect(text).toContain('Open tickets');
    expect(text).toContain('Escalated tickets');
    expect(text).toContain('Breached tickets');
    expect(text).toContain('At risk tickets');
  });

  it('shows empty state when all counts are zero', () => {
    dashboardService.getOperationalDashboard.and.returnValue(of(buildDashboard()));

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No operational metrics found for the selected filters.');
  });

  it('shows error state on API failure', () => {
    dashboardService.getOperationalDashboard.and.returnValue(throwError(() => new Error('boom')));

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Dashboard failed.');
  });

  it('filter by status calls service with selected filters', () => {
    dashboardService.getOperationalDashboard.and.returnValue(of(buildDashboard()));
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    fixture.componentInstance.filters.status = 'Open';
    fixture.componentInstance.filters.agentUserId = 'agent-1';
    fixture.componentInstance.loadDashboard();

    expect(dashboardService.getOperationalDashboard).toHaveBeenCalledWith(jasmine.objectContaining({
      status: 'Open',
      agentUserId: 'agent-1'
    }));
  });

  it('drill-in query routes to tickets with query params', () => {
    dashboardService.getOperationalDashboard.and.returnValue(of(buildDashboard()));
    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    fixture.componentInstance.filters.accountId = 'account-1';
    const query = fixture.componentInstance.ticketQuery({ status: 'Open', isEscalated: true });

    expect(query).toEqual(jasmine.objectContaining({
      accountId: 'account-1',
      status: 'Open',
      isEscalated: true
    }));
  });
});

function buildDashboard(overrides: Partial<OperationalDashboard> = {}): OperationalDashboard {
  return {
    generatedAtUtc: '2026-05-18T00:00:00Z',
    totalTicketCount: 0,
    openTicketCount: 0,
    assignedTicketCount: 0,
    escalatedTicketCount: 0,
    breachedTicketCount: 0,
    atRiskTicketCount: 0,
    ticketsByStatus: [{ label: 'Open', key: 'Open', count: 0, status: 'Open' }],
    ticketsByPriority: [],
    ticketsBySlaState: [],
    ticketsByAccount: [],
    ticketsByCampaign: [],
    ticketsByAssignedAgent: [],
    ticketsBySupervisor: [],
    appliedFilters: {},
    ...overrides
  };
}
