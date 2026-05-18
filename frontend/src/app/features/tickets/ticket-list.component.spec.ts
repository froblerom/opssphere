import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { OrganizationService } from '../organization/organization.service';
import { TicketListComponent } from './ticket-list.component';
import { TicketService } from './ticket.service';

describe('TicketListComponent', () => {
  let ticketService: jasmine.SpyObj<Pick<TicketService, 'getTickets'>>;
  let authService: jasmine.SpyObj<Pick<AuthService, 'hasPermission'>>;
  let organizationService: jasmine.SpyObj<Pick<OrganizationService, 'getRegions' | 'getCountries' | 'getAccounts' | 'getCampaigns'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let fixture: ComponentFixture<TicketListComponent>;

  beforeEach(() => {
    ticketService = jasmine.createSpyObj<Pick<TicketService, 'getTickets'>>('TicketService', ['getTickets']);
    authService = jasmine.createSpyObj<Pick<AuthService, 'hasPermission'>>('AuthService', ['hasPermission']);
    organizationService = jasmine.createSpyObj<Pick<OrganizationService, 'getRegions' | 'getCountries' | 'getAccounts' | 'getCampaigns'>>(
      'OrganizationService',
      ['getRegions', 'getCountries', 'getAccounts', 'getCampaigns']
    );
    errorParser = jasmine.createSpyObj<Pick<ApiErrorParserService, 'parse'>>('ApiErrorParserService', ['parse']);
    ticketService.getTickets.and.returnValue(of([]));
    authService.hasPermission.and.returnValue(false);
    organizationService.getRegions.and.returnValue(of([]));
    organizationService.getCountries.and.returnValue(of([]));
    organizationService.getAccounts.and.returnValue(of([]));
    organizationService.getCampaigns.and.returnValue(of([]));
    errorParser.parse.and.returnValue({ code: 'server_error', message: 'Tickets failed safely.', details: [] });

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
        { provide: AuthService, useValue: authService },
        { provide: OrganizationService, useValue: organizationService },
        { provide: ApiErrorParserService, useValue: errorParser }
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

  it('uses ApiErrorParserService for safe load errors', () => {
    ticketService.getTickets.and.returnValue(throwError(() => new Error('raw failure')));

    fixture.componentInstance.loadTickets();

    expect(errorParser.parse).toHaveBeenCalled();
    expect(fixture.componentInstance.error).toBe('Tickets failed safely.');
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

    fixture = TestBed.createComponent(TicketListComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredCountries()).toEqual(countries);
    fixture.componentInstance.filters.accountId = 'account-1';
    expect(fixture.componentInstance.filteredCampaigns()).toEqual(campaigns);
  });
});
