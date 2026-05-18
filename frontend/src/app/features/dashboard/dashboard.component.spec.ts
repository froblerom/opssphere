import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { NEVER, of, throwError } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { OrganizationService } from '../organization/organization.service';
import { DashboardComponent } from './dashboard.component';
import { OperationalDashboard } from './dashboard.models';
import { DashboardService } from './dashboard.service';

describe('DashboardComponent', () => {
  let dashboardService: jasmine.SpyObj<Pick<DashboardService, 'getOperationalDashboard'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let organizationService: jasmine.SpyObj<Pick<OrganizationService, 'getRegions' | 'getCountries' | 'getAccounts' | 'getCampaigns'>>;
  let fixture: ComponentFixture<DashboardComponent>;

  beforeEach(() => {
    dashboardService = jasmine.createSpyObj<Pick<DashboardService, 'getOperationalDashboard'>>('DashboardService', ['getOperationalDashboard']);
    errorParser = jasmine.createSpyObj<Pick<ApiErrorParserService, 'parse'>>('ApiErrorParserService', ['parse']);
    organizationService = jasmine.createSpyObj<Pick<OrganizationService, 'getRegions' | 'getCountries' | 'getAccounts' | 'getCampaigns'>>(
      'OrganizationService',
      ['getRegions', 'getCountries', 'getAccounts', 'getCampaigns']
    );
    errorParser.parse.and.returnValue({ code: 'server_error', message: 'Dashboard failed.', details: [] });
    organizationService.getRegions.and.returnValue(of([]));
    organizationService.getCountries.and.returnValue(of([]));
    organizationService.getAccounts.and.returnValue(of([]));
    organizationService.getCampaigns.and.returnValue(of([]));

    TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: DashboardService, useValue: dashboardService },
        { provide: ApiErrorParserService, useValue: errorParser },
        { provide: OrganizationService, useValue: organizationService }
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

  it('loads organization scope filter dropdown options', () => {
    const regions = [{ id: 'region-1', code: 'LATAM', name: 'Latin America', isActive: true, createdAt: '2026-05-18T00:00:00Z' }];
    const countries = [{ id: 'country-1', code: 'MX', name: 'Mexico', regionId: 'region-1', regionCode: 'LATAM', regionName: 'Latin America', isActive: true, createdAt: '2026-05-18T00:00:00Z' }];
    const accounts = [{ id: 'account-1', code: 'NOVABANK', name: 'NovaBank', countryId: 'country-1', countryCode: 'MX', countryName: 'Mexico', regionId: 'region-1', regionCode: 'LATAM', description: null, isActive: true, createdAt: '2026-05-18T00:00:00Z' }];
    const campaigns = [{ id: 'campaign-1', code: 'NOVABANK-CC', name: 'Credit Card Support', accountId: 'account-1', accountCode: 'NOVABANK', accountName: 'NovaBank', countryId: 'country-1', countryCode: 'MX', countryName: 'Mexico', regionId: 'region-1', regionCode: 'LATAM', description: null, isActive: true, createdAt: '2026-05-18T00:00:00Z' }];
    organizationService.getRegions.and.returnValue(of(regions));
    organizationService.getCountries.and.returnValue(of(countries));
    organizationService.getAccounts.and.returnValue(of(accounts));
    organizationService.getCampaigns.and.returnValue(of(campaigns));
    dashboardService.getOperationalDashboard.and.returnValue(of(buildDashboard()));

    fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredCountries()).toEqual(countries);
    fixture.componentInstance.filters.regionId = 'region-1';
    expect(fixture.componentInstance.filteredAccounts()).toEqual(accounts);
    expect(fixture.componentInstance.filteredCampaigns()).toEqual(campaigns);
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
