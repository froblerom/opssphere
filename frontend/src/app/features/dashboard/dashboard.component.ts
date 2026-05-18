import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { forkJoin } from 'rxjs';

import { SafeApiError } from '../../core/models/api-error.models';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { Account, Campaign, Country, Region } from '../organization/organization.models';
import { OrganizationService } from '../organization/organization.service';
import {
  DashboardEntityGroupItem,
  DashboardFilters,
  DashboardGroupItem,
  DashboardUserGroupItem,
  OperationalDashboard
} from './dashboard.models';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Operational Dashboard</h1>
      </div>

      <form class="dashboard-filters" (ngSubmit)="loadDashboard()">
        <label>
          Status
          <select name="status" [(ngModel)]="filters.status">
            <option value="">All</option>
            <option *ngFor="let status of statusOptions" [value]="status">{{ status }}</option>
          </select>
        </label>

        <label>
          Priority
          <select name="priority" [(ngModel)]="filters.priority">
            <option value="">All</option>
            <option *ngFor="let priority of priorityOptions" [value]="priority">{{ priority }}</option>
          </select>
        </label>

        <label>
          SLA
          <select name="slaState" [(ngModel)]="filters.slaState">
            <option value="">All</option>
            <option *ngFor="let state of slaStateOptions" [value]="state">{{ labelSla(state) }}</option>
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
          <select name="campaignId" [(ngModel)]="filters.campaignId">
            <option value="">All</option>
            <option *ngFor="let campaign of filteredCampaigns()" [value]="campaign.id">{{ campaign.name }}</option>
          </select>
        </label>

        <label>
          Agent User ID
          <input name="agentUserId" type="text" [(ngModel)]="filters.agentUserId" />
        </label>

        <label>
          Supervisor User ID
          <input name="supervisorUserId" type="text" [(ngModel)]="filters.supervisorUserId" />
        </label>

        <label>
          From
          <input name="dateFrom" type="datetime-local" [(ngModel)]="filters.dateFrom" />
        </label>

        <label>
          To
          <input name="dateTo" type="datetime-local" [(ngModel)]="filters.dateTo" />
        </label>

        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">Apply</button>
          <button type="button" class="btn btn-secondary" (click)="resetFilters()" [disabled]="loading">Reset</button>
        </div>
      </form>

      <div *ngIf="loading" class="loading">Loading dashboard...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <ng-container *ngIf="!loading && !error && dashboard as model">
        <p class="timestamp">Generated {{ model.generatedAtUtc | date:'medium' }}</p>

        <div *ngIf="isEmpty(model)" class="empty-state">
          No operational metrics found for the selected filters.
        </div>

        <section class="summary-grid" aria-label="Operational summary">
          <a class="stat-card" [routerLink]="['/tickets']" [queryParams]="ticketQuery()">
            <mat-icon aria-hidden="true">confirmation_number</mat-icon>
            <span>Total tickets</span>
            <strong>{{ model.totalTicketCount }}</strong>
          </a>
          <a class="stat-card" [routerLink]="['/tickets']" [queryParams]="ticketQuery({ status: 'Open' })">
            <mat-icon aria-hidden="true">inbox</mat-icon>
            <span>Open tickets</span>
            <strong>{{ model.openTicketCount }}</strong>
          </a>
          <a class="stat-card" [routerLink]="['/tickets']" [queryParams]="ticketQuery()">
            <mat-icon aria-hidden="true">assignment_ind</mat-icon>
            <span>Assigned tickets</span>
            <strong>{{ model.assignedTicketCount }}</strong>
          </a>
          <a class="stat-card" [routerLink]="['/tickets']" [queryParams]="ticketQuery({ isEscalated: true })">
            <mat-icon aria-hidden="true">priority_high</mat-icon>
            <span>Escalated tickets</span>
            <strong>{{ model.escalatedTicketCount }}</strong>
          </a>
          <a class="stat-card" [routerLink]="['/tickets']" [queryParams]="ticketQuery({ slaState: 'Breached' })">
            <mat-icon aria-hidden="true">report</mat-icon>
            <span>Breached tickets</span>
            <strong>{{ model.breachedTicketCount }}</strong>
          </a>
          <a class="stat-card" [routerLink]="['/tickets']" [queryParams]="ticketQuery({ slaState: 'AtRisk' })">
            <mat-icon aria-hidden="true">schedule</mat-icon>
            <span>At risk tickets</span>
            <strong>{{ model.atRiskTicketCount }}</strong>
          </a>
        </section>

        <section class="widget-grid">
          <article class="widget">
            <h2>Tickets by status</h2>
            <ng-container *ngTemplateOutlet="groupList; context: { items: model.ticketsByStatus }" />
          </article>

          <article class="widget">
            <h2>Tickets by priority</h2>
            <ng-container *ngTemplateOutlet="groupList; context: { items: model.ticketsByPriority }" />
          </article>

          <article class="widget">
            <h2>Tickets by SLA state</h2>
            <ng-container *ngTemplateOutlet="groupList; context: { items: model.ticketsBySlaState }" />
          </article>

          <article class="widget">
            <h2>Tickets by account</h2>
            <ng-container *ngTemplateOutlet="entityList; context: { items: model.ticketsByAccount }" />
          </article>

          <article class="widget">
            <h2>Tickets by campaign</h2>
            <ng-container *ngTemplateOutlet="entityList; context: { items: model.ticketsByCampaign }" />
          </article>

          <article class="widget">
            <h2>Workload by assigned agent</h2>
            <ng-container *ngTemplateOutlet="userList; context: { items: model.ticketsByAssignedAgent }" />
          </article>

          <article class="widget" *ngIf="model.ticketsBySupervisor.length > 0">
            <h2>Workload by supervisor</h2>
            <ng-container *ngTemplateOutlet="userList; context: { items: model.ticketsBySupervisor }" />
          </article>
        </section>
      </ng-container>

      <ng-template #groupList let-items="items">
        <p *ngIf="items.length === 0" class="muted">No records.</p>
        <ul *ngIf="items.length > 0" class="metric-list">
          <li *ngFor="let item of items">
            <a [routerLink]="['/tickets']" [queryParams]="ticketQuery(item)">
              <span>{{ item.label }}</span>
              <strong>{{ item.count }}</strong>
            </a>
          </li>
        </ul>
      </ng-template>

      <ng-template #entityList let-items="items">
        <p *ngIf="items.length === 0" class="muted">No records.</p>
        <ul *ngIf="items.length > 0" class="metric-list">
          <li *ngFor="let item of items">
            <a [routerLink]="['/tickets']" [queryParams]="ticketQuery(item)">
              <span>{{ item.label }}</span>
              <strong>{{ item.count }}</strong>
            </a>
          </li>
        </ul>
      </ng-template>

      <ng-template #userList let-items="items">
        <p *ngIf="items.length === 0" class="muted">No records.</p>
        <ul *ngIf="items.length > 0" class="metric-list">
          <li *ngFor="let item of items">
            <a [routerLink]="['/tickets']" [queryParams]="ticketQuery(item)">
              <span>{{ item.label }}</span>
              <strong>{{ item.count }}</strong>
            </a>
          </li>
        </ul>
      </ng-template>
    </div>
  `,
  styles: [`
    .dashboard-filters {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 12px;
      align-items: end;
      margin: 0 0 18px;
    }

    .dashboard-filters label {
      display: grid;
      gap: 4px;
      font-size: 0.9rem;
      font-weight: 600;
    }

    .actions {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .timestamp, .muted {
      color: #6b7280;
      margin: 0 0 12px;
    }

    .empty-state {
      border: 1px solid #d1d5db;
      border-radius: 8px;
      padding: 14px;
      margin-bottom: 16px;
      color: #374151;
      background: #f9fafb;
    }

    .summary-grid, .widget-grid {
      display: grid;
      gap: 14px;
      margin-bottom: 18px;
    }

    .summary-grid {
      grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
    }

    .widget-grid {
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
    }

    .stat-card, .widget {
      border: 1px solid #d1d5db;
      border-radius: 8px;
      background: #fff;
    }

    .stat-card {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 4px 10px;
      align-items: center;
      padding: 14px;
      color: inherit;
      text-decoration: none;
    }

    .stat-card mat-icon {
      grid-row: span 2;
      color: #2563eb;
    }

    .stat-card span {
      color: #4b5563;
      font-size: 0.9rem;
    }

    .stat-card strong {
      font-size: 1.6rem;
      line-height: 1;
    }

    .widget {
      padding: 14px;
    }

    .widget h2 {
      margin: 0 0 10px;
      font-size: 1rem;
    }

    .metric-list {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      gap: 8px;
    }

    .metric-list a {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      color: inherit;
      text-decoration: none;
    }
  `]
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly errorParser = inject(ApiErrorParserService);
  private readonly organizationService = inject(OrganizationService);

  readonly statusOptions = ['Open', 'Assigned', 'InProgress', 'WaitingForCustomer', 'Escalated', 'Resolved', 'Closed'];
  readonly priorityOptions = ['Low', 'Normal', 'High', 'Critical'];
  readonly slaStateOptions = ['WithinSla', 'AtRisk', 'Breached', 'Completed'];

  dashboard: OperationalDashboard | null = null;
  regions: Region[] = [];
  countries: Country[] = [];
  accounts: Account[] = [];
  campaigns: Campaign[] = [];
  filters: DashboardFilters = {};
  loading = true;
  error: string | null = null;

  ngOnInit() {
    this.loadScopeOptions();
    this.loadDashboard();
  }

  loadDashboard() {
    this.loading = true;
    this.error = null;

    this.dashboardService.getOperationalDashboard(this.cleanFilters()).subscribe({
      next: (dashboard) => {
        this.dashboard = dashboard;
        this.loading = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.error = parsed.message;
        this.dashboard = null;
        this.loading = false;
      }
    });
  }

  resetFilters() {
    this.filters = {};
    this.loadDashboard();
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

  isEmpty(dashboard: OperationalDashboard) {
    return dashboard.totalTicketCount === 0;
  }

  labelSla(state: string) {
    return state === 'WithinSla' ? 'Within SLA' : state === 'AtRisk' ? 'At Risk' : state;
  }

  ticketQuery(item?: Partial<DashboardGroupItem & DashboardEntityGroupItem & DashboardUserGroupItem> & { isEscalated?: boolean }) {
    const query: Record<string, string | boolean> = {};
    this.assign(query, this.cleanFilters());
    this.assign(query, item ?? {});
    if ('agentUserId' in query && !('assignedAgentUserId' in query)) {
      query['assignedAgentUserId'] = query['agentUserId'];
      delete query['agentUserId'];
    }

    return query;
  }

  private cleanFilters(): DashboardFilters {
    return {
      regionId: this.clean(this.filters.regionId),
      countryId: this.clean(this.filters.countryId),
      accountId: this.clean(this.filters.accountId),
      campaignId: this.clean(this.filters.campaignId),
      supervisorUserId: this.clean(this.filters.supervisorUserId),
      agentUserId: this.clean(this.filters.agentUserId),
      status: this.clean(this.filters.status),
      priority: this.clean(this.filters.priority),
      slaState: this.clean(this.filters.slaState),
      dateFrom: this.clean(this.filters.dateFrom),
      dateTo: this.clean(this.filters.dateTo)
    };
  }

  private clean(value?: string | null) {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private assign(target: Record<string, string | boolean>, source: object) {
    const allowedKeys = new Set([
      'regionId',
      'countryId',
      'accountId',
      'campaignId',
      'supervisorUserId',
      'agentUserId',
      'assignedAgentUserId',
      'status',
      'priority',
      'slaState',
      'dateFrom',
      'dateTo',
      'isEscalated'
    ]);

    Object.entries(source).forEach(([key, value]) => {
      if (allowedKeys.has(key) && value !== undefined && value !== null && value !== '') {
        target[key] = value as string | boolean;
      }
    });
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
