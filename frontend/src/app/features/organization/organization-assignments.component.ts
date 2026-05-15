import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { UserSummary } from '../users/user-management.models';
import { UserManagementService } from '../users/user-management.service';
import { Account, Campaign, Country, Region, ScopeRequest, UserScope } from './organization.models';
import { OrganizationService } from './organization.service';

@Component({
  selector: 'app-organization-assignments',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatSelectModule],
  template: `
    <section class="assignments" aria-labelledby="assignments-title">
      <header>
        <div>
          <p class="eyebrow">Organization</p>
          <h1 id="assignments-title">Assignments</h1>
        </div>
        <a mat-stroked-button routerLink="/admin/organization">
          <mat-icon aria-hidden="true">arrow_back</mat-icon>
          <span>Organization</span>
        </a>
      </header>

      @if (errorMessage()) {
        <p class="error" role="alert">{{ errorMessage() }}</p>
      }

      <form [formGroup]="userForm" class="user-form">
        <mat-form-field appearance="outline">
          <mat-label>User</mat-label>
          <mat-select formControlName="userId" (selectionChange)="loadUserScopes()">
            @for (user of activeUsers(); track user.id) {
              <mat-option [value]="user.id">{{ user.displayName }} - {{ roleNames(user) }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      </form>

      @if (selectedUserId()) {
        <form [formGroup]="scopeForm" (ngSubmit)="addScope()" class="scope-form">
          <mat-form-field appearance="outline">
            <mat-label>Scope</mat-label>
            <mat-select formControlName="scopeType" (selectionChange)="scopeForm.controls.targetId.setValue('')">
              <mat-option value="Region">Region</mat-option>
              <mat-option value="Country">Country</mat-option>
              <mat-option value="Account">Account</mat-option>
              <mat-option value="Campaign">Campaign</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Target</mat-label>
            <mat-select formControlName="targetId">
              @for (target of targetOptions(); track target.id) {
                <mat-option [value]="target.id">{{ target.code }} - {{ target.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <button mat-flat-button color="primary" type="submit" [disabled]="scopeForm.invalid">
            <mat-icon aria-hidden="true">add</mat-icon>
            <span>Add</span>
          </button>
        </form>

        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Scope</th>
                <th>Target</th>
                <th>Status</th>
                <th aria-label="Actions"></th>
              </tr>
            </thead>
            <tbody>
              @for (scope of scopes(); track scopeKey(scope)) {
                <tr>
                  <td>{{ scope.scopeType }}</td>
                  <td>{{ scopeLabel(scope) }}</td>
                  <td><span class="status" [class.inactive]="!scope.isActive">{{ scope.isActive ? 'Active' : 'Inactive' }}</span></td>
                  <td class="row-actions">
                    <button mat-icon-button type="button" aria-label="Remove" (click)="removeScope(scope)">
                      <mat-icon aria-hidden="true">delete</mat-icon>
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr><td colspan="4" class="empty">No scopes assigned.</td></tr>
              }
            </tbody>
          </table>
        </div>

        <div class="actions">
          <button mat-flat-button color="primary" type="button" [disabled]="isSaving()" (click)="save()">
            <mat-icon aria-hidden="true">save</mat-icon>
            <span>Save</span>
          </button>
        </div>
      }
    </section>
  `,
  styles: [`
    .assignments { display: grid; gap: 1rem; }
    header { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
    h1 { margin: 0; color: #111827; font-size: 1.65rem; line-height: 1.2; }
    .eyebrow { margin: 0 0 0.2rem; color: #5b6475; font-size: 0.8rem; font-weight: 700; text-transform: uppercase; }
    .user-form { max-width: 680px; }
    .scope-form { display: grid; max-width: 820px; grid-template-columns: minmax(0, 1fr) minmax(0, 1.5fr) auto; align-items: center; gap: 0.75rem; }
    .table-wrap { overflow: auto; border: 1px solid #d8dee8; border-radius: 8px; background: #fff; }
    table { width: 100%; min-width: 680px; border-collapse: collapse; }
    th, td { padding: 0.8rem 0.9rem; border-bottom: 1px solid #e8edf5; text-align: left; vertical-align: middle; }
    th { color: #3d4758; font-size: 0.78rem; font-weight: 700; text-transform: uppercase; }
    tr:last-child td { border-bottom: 0; }
    .status { display: inline-block; min-width: 4.6rem; border-radius: 999px; padding: 0.2rem 0.55rem; background: #e8f6ef; color: #166534; text-align: center; font-size: 0.78rem; font-weight: 700; }
    .status.inactive { background: #f2f4f7; color: #5b6475; }
    .actions { display: flex; gap: 0.75rem; }
    .row-actions { width: 4rem; }
    .error, .empty { color: #b91c1c; }
    @media (max-width: 720px) { .scope-form { grid-template-columns: 1fr; } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrganizationAssignmentsComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly organization = inject(OrganizationService);
  private readonly usersService = inject(UserManagementService);
  private readonly errorParser = inject(ApiErrorParserService);

  protected readonly users = signal<UserSummary[]>([]);
  protected readonly regions = signal<Region[]>([]);
  protected readonly countries = signal<Country[]>([]);
  protected readonly accounts = signal<Account[]>([]);
  protected readonly campaigns = signal<Campaign[]>([]);
  protected readonly scopes = signal<UserScope[]>([]);
  protected readonly errorMessage = signal('');
  protected readonly isSaving = signal(false);

  protected readonly activeUsers = computed(() => this.users().filter((u) => u.isActive));
  protected readonly selectedUserId = computed(() => this.userForm.controls.userId.value);
  protected readonly targetOptions = computed(() => {
    const scopeType = this.scopeForm.controls.scopeType.value;
    if (scopeType === 'Region') return this.regions().filter((item) => item.isActive);
    if (scopeType === 'Country') return this.countries().filter((item) => item.isActive);
    if (scopeType === 'Account') return this.accounts().filter((item) => item.isActive);
    return this.campaigns().filter((item) => item.isActive);
  });

  protected readonly userForm = this.formBuilder.nonNullable.group({
    userId: ['', Validators.required]
  });

  protected readonly scopeForm = this.formBuilder.nonNullable.group({
    scopeType: ['Region', Validators.required],
    targetId: ['', Validators.required]
  });

  constructor() {
    forkJoin({
      users: this.usersService.getUsers(),
      regions: this.organization.getRegions(),
      countries: this.organization.getCountries(),
      accounts: this.organization.getAccounts(),
      campaigns: this.organization.getCampaigns()
    }).subscribe({
      next: ({ users, regions, countries, accounts, campaigns }) => {
        this.users.set(users);
        this.regions.set(regions);
        this.countries.set(countries);
        this.accounts.set(accounts);
        this.campaigns.set(campaigns);
      },
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }

  protected roleNames(user: UserSummary): string {
    return user.roles.map((role) => role.name).join(', ') || 'Unassigned';
  }

  protected loadUserScopes(): void {
    const userId = this.selectedUserId();
    if (!userId) return;
    this.organization.getUserScopes(userId).subscribe({
      next: (assignment) => this.scopes.set(assignment.scopes.filter((scope) => scope.isActive)),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }

  protected addScope(): void {
    if (this.scopeForm.invalid) return;
    const scope = this.toScope(this.scopeForm.getRawValue().scopeType, this.scopeForm.getRawValue().targetId);
    if (this.scopes().some((existing) => this.scopeKey(existing) === this.scopeKey(scope))) return;
    this.scopes.set([...this.scopes(), scope]);
  }

  protected removeScope(scope: UserScope): void {
    this.scopes.set(this.scopes().filter((existing) => this.scopeKey(existing) !== this.scopeKey(scope)));
  }

  protected save(): void {
    const userId = this.selectedUserId();
    if (!userId) return;
    this.isSaving.set(true);
    this.organization.updateUserScopes(userId, { scopes: this.scopes().map((scope) => this.toRequest(scope)) }).subscribe({
      next: (assignment) => {
        this.scopes.set(assignment.scopes.filter((scope) => scope.isActive));
        this.isSaving.set(false);
      },
      error: (error) => {
        this.errorMessage.set(this.errorParser.parse(error).message);
        this.isSaving.set(false);
      }
    });
  }

  protected scopeKey(scope: ScopeRequest): string {
    return `${scope.scopeType}:${scope.regionId ?? scope.countryId ?? scope.accountId ?? scope.campaignId}`;
  }

  protected scopeLabel(scope: ScopeRequest): string {
    if (scope.scopeType === 'Region') return this.label(this.regions(), scope.regionId);
    if (scope.scopeType === 'Country') return this.label(this.countries(), scope.countryId);
    if (scope.scopeType === 'Account') return this.label(this.accounts(), scope.accountId);
    return this.label(this.campaigns(), scope.campaignId);
  }

  private toScope(scopeType: string, targetId: string): UserScope {
    return {
      id: `${scopeType}:${targetId}`,
      scopeType,
      regionId: scopeType === 'Region' ? targetId : null,
      countryId: scopeType === 'Country' ? targetId : null,
      accountId: scopeType === 'Account' ? targetId : null,
      campaignId: scopeType === 'Campaign' ? targetId : null,
      isActive: true,
      createdAt: new Date().toISOString()
    };
  }

  private toRequest(scope: ScopeRequest): ScopeRequest {
    return {
      scopeType: scope.scopeType,
      regionId: scope.regionId ?? null,
      countryId: scope.countryId ?? null,
      accountId: scope.accountId ?? null,
      campaignId: scope.campaignId ?? null
    };
  }

  private label(items: Array<Region | Country | Account | Campaign>, id?: string | null): string {
    const item = items.find((candidate) => candidate.id === id);
    return item ? `${item.code} - ${item.name}` : 'Unknown';
  }
}
