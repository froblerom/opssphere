import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';

import { AppPermissions } from '../../core/auth/auth-permissions';
import { AuthService } from '../../core/auth/auth.service';
import { SafeApiError } from '../../core/models/api-error.models';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { Account, Campaign, Country, Region } from '../organization/organization.models';
import { OrganizationService } from '../organization/organization.service';
import { SlaStateBadgeComponent } from '../../shared/components/sla-state-badge.component';
import { TicketService } from './ticket.service';
import { TicketListFilter, TicketListItem } from './ticket.models';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SlaStateBadgeComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Tickets</h1>
        <a *ngIf="canCreate" routerLink="/tickets/create" class="btn btn-primary">New Ticket</a>
      </div>

      <form class="ticket-filters" (ngSubmit)="loadTickets()">
        <label>
          Status
          <select name="status" [(ngModel)]="filters.status" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let status of statusOptions" [value]="status">{{ status }}</option>
          </select>
        </label>

        <label>
          Priority
          <select name="priority" [(ngModel)]="filters.priority" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let priority of priorityOptions" [value]="priority">{{ priority }}</option>
          </select>
        </label>

        <label>
          SLA
          <select name="slaState" [(ngModel)]="filters.slaState" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let state of slaStateOptions" [value]="state">{{ slaStateLabel(state) }}</option>
          </select>
        </label>

        <label>
          Region
          <select name="regionId" [(ngModel)]="filters.regionId" (change)="onScopeFilterChange('region')">
            <option value="">All</option>
            <option *ngFor="let region of regions" [value]="region.id">{{ region.name }}</option>
          </select>
        </label>

        <label>
          Country
          <select name="countryId" [(ngModel)]="filters.countryId" (change)="onScopeFilterChange('country')">
            <option value="">All</option>
            <option *ngFor="let country of filteredCountries()" [value]="country.id">{{ country.name }}</option>
          </select>
        </label>

        <label>
          Account
          <select name="accountId" [(ngModel)]="filters.accountId" (change)="onScopeFilterChange('account')">
            <option value="">All</option>
            <option *ngFor="let account of filteredAccounts()" [value]="account.id">{{ account.name }}</option>
          </select>
        </label>

        <label>
          Campaign
          <select name="campaignId" [(ngModel)]="filters.campaignId" (change)="loadTickets()">
            <option value="">All</option>
            <option *ngFor="let campaign of filteredCampaigns()" [value]="campaign.id">{{ campaign.name }}</option>
          </select>
        </label>

        <button type="button" class="btn btn-secondary" (click)="clearFilters()">Clear</button>
      </form>

      <div *ngIf="loading" class="loading">Loading tickets...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <table *ngIf="!loading && tickets.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Ticket #</th>
            <th>Customer</th>
            <th>Campaign</th>
            <th>Priority</th>
            <th>Status</th>
            <th>SLA State</th>
            <th>Created</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let ticket of tickets">
            <td>{{ ticket.ticketNumber }}</td>
            <td>{{ ticket.customerName }}</td>
            <td>{{ ticket.campaignName }}</td>
            <td>{{ ticket.priority }}</td>
            <td>{{ ticket.status }}</td>
            <td><app-sla-state-badge [slaState]="ticket.slaState" /></td>
            <td>{{ ticket.createdAt | date:'medium' }}</td>
            <td>
              <a [routerLink]="['/tickets', ticket.id]">View</a>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && tickets.length === 0">No tickets found.</p>
    </div>
  `,
  styles: [`
    .ticket-filters {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
      align-items: end;
      margin: 0 0 16px;
    }

    .ticket-filters label {
      display: grid;
      gap: 4px;
      font-size: 0.9rem;
      font-weight: 600;
    }

    .ticket-filters select {
      min-width: 150px;
    }

  `]
})
export class TicketListComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly organizationService = inject(OrganizationService);
  private readonly errorParser = inject(ApiErrorParserService);

  tickets: TicketListItem[] = [];
  regions: Region[] = [];
  countries: Country[] = [];
  accounts: Account[] = [];
  campaigns: Campaign[] = [];
  readonly statusOptions = ['Open', 'Assigned', 'InProgress', 'WaitingForCustomer', 'Escalated', 'Resolved', 'Closed'];
  readonly priorityOptions = ['Low', 'Normal', 'High', 'Critical'];
  readonly slaStateOptions = ['WithinSla', 'AtRisk', 'Breached', 'Completed'];
  filters: TicketListFilter = {
    status: '',
    priority: '',
    slaState: ''
  };
  loading = true;
  error: string | null = null;

  get canCreate() {
    return this.authService.hasPermission(AppPermissions.TicketsCreate);
  }

  ngOnInit() {
    this.applyQueryParams();
    this.loadScopeOptions();
    this.loadTickets();
  }

  loadTickets() {
    this.loading = true;
    this.error = null;

    this.ticketService.getTickets(this.filters).subscribe({
      next: (tickets) => {
        this.tickets = tickets;
        this.loading = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.error = parsed.message || 'Unable to load tickets.';
        this.loading = false;
      }
    });
  }

  clearFilters() {
    this.filters = { status: '', priority: '', slaState: '' };
    this.loadTickets();
  }

  onScopeFilterChange(level: 'region' | 'country' | 'account') {
    if (level === 'region') {
      this.filters.countryId = '';
      this.filters.accountId = '';
      this.filters.campaignId = '';
    }

    if (level === 'country') {
      this.filters.accountId = '';
      this.filters.campaignId = '';
    }

    if (level === 'account') {
      this.filters.campaignId = '';
    }

    this.loadTickets();
  }

  filteredCountries() {
    return this.countries.filter((country) => !this.filters.regionId || country.regionId === this.filters.regionId);
  }

  filteredAccounts() {
    return this.accounts.filter((account) => {
      if (this.filters.countryId) return account.countryId === this.filters.countryId;
      if (this.filters.regionId) return account.regionId === this.filters.regionId;
      return true;
    });
  }

  filteredCampaigns() {
    return this.campaigns.filter((campaign) => {
      if (this.filters.accountId) return campaign.accountId === this.filters.accountId;
      if (this.filters.countryId) return campaign.countryId === this.filters.countryId;
      if (this.filters.regionId) return campaign.regionId === this.filters.regionId;
      return true;
    });
  }

  slaStateLabel(state: string) {
    return state === 'WithinSla' ? 'Within SLA' : state === 'AtRisk' ? 'At Risk' : state;
  }

  private applyQueryParams() {
    const params = this.route.snapshot.queryParamMap;
    this.filters = {
      status: params.get('status') ?? '',
      priority: params.get('priority') ?? '',
      slaState: params.get('slaState') ?? '',
      regionId: params.get('regionId'),
      countryId: params.get('countryId'),
      accountId: params.get('accountId'),
      campaignId: params.get('campaignId'),
      supervisorUserId: params.get('supervisorUserId'),
      assignedAgentUserId: params.get('assignedAgentUserId') ?? params.get('agentUserId'),
      isEscalated: params.get('isEscalated'),
      dateFrom: params.get('dateFrom'),
      dateTo: params.get('dateTo')
    };
  }

  private loadScopeOptions() {
    forkJoin({
      regions: this.organizationService.getRegions(),
      countries: this.organizationService.getCountries(),
      accounts: this.organizationService.getAccounts(),
      campaigns: this.organizationService.getCampaigns()
    }).subscribe({
      next: ({ regions, countries, accounts, campaigns }) => {
        this.regions = regions.filter((item) => item.isActive);
        this.countries = countries.filter((item) => item.isActive);
        this.accounts = accounts.filter((item) => item.isActive);
        this.campaigns = campaigns.filter((item) => item.isActive);
      },
      error: () => {
        // Scope dropdowns are a demo usability aid; backend filters remain source of truth.
      }
    });
  }
}
