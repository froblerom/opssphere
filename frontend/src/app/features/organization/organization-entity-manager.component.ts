import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { Account, Campaign, Country, EntityKind, OrganizationEntity, Region } from './organization.models';
import { OrganizationService } from './organization.service';

@Component({
  selector: 'app-organization-entity-manager',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule],
  template: `
    <section class="manager" aria-labelledby="manager-title">
      <header>
        <div>
          <p class="eyebrow">Organization</p>
          <h1 id="manager-title">{{ title() }}</h1>
        </div>
        <a mat-stroked-button routerLink="/admin/organization">
          <mat-icon aria-hidden="true">arrow_back</mat-icon>
          <span>Organization</span>
        </a>
      </header>

      @if (errorMessage()) {
        <p class="error" role="alert">{{ errorMessage() }}</p>
      }

      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        @if (kind() === 'countries') {
          <mat-form-field appearance="outline">
            <mat-label>Region</mat-label>
            <mat-select formControlName="regionId">
              @for (region of activeRegions(); track region.id) {
                <mat-option [value]="region.id">{{ region.code }} - {{ region.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }

        @if (kind() === 'accounts') {
          <mat-form-field appearance="outline">
            <mat-label>Country</mat-label>
            <mat-select formControlName="countryId">
              @for (country of activeCountries(); track country.id) {
                <mat-option [value]="country.id">{{ country.code }} - {{ country.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }

        @if (kind() === 'campaigns') {
          <mat-form-field appearance="outline">
            <mat-label>Account</mat-label>
            <mat-select formControlName="accountId" (selectionChange)="syncCampaignCountry()">
              @for (account of activeAccounts(); track account.id) {
                <mat-option [value]="account.id">{{ account.code }} - {{ account.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Country</mat-label>
            <mat-select formControlName="countryId">
              @for (country of activeCountries(); track country.id) {
                <mat-option [value]="country.id">{{ country.code }} - {{ country.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }

        <mat-form-field appearance="outline">
          <mat-label>Code</mat-label>
          <input matInput autocomplete="off" formControlName="code" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput autocomplete="off" formControlName="name" />
        </mat-form-field>

        @if (kind() === 'accounts' || kind() === 'campaigns') {
          <mat-form-field appearance="outline">
            <mat-label>Description</mat-label>
            <textarea matInput rows="2" formControlName="description"></textarea>
          </mat-form-field>
        }

        <div class="actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || isSaving()">
            <mat-icon aria-hidden="true">save</mat-icon>
            <span>{{ editingId() ? 'Update' : 'Create' }}</span>
          </button>
          @if (editingId()) {
            <button mat-button type="button" (click)="resetForm()">Cancel</button>
          }
        </div>
      </form>

      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Parent</th>
              <th>Status</th>
              <th aria-label="Actions"></th>
            </tr>
          </thead>
          <tbody>
            @for (item of entities(); track item.id) {
              <tr>
                <td>{{ item.code }}</td>
                <td>{{ item.name }}</td>
                <td>{{ parentLabel(item) }}</td>
                <td><span class="status" [class.inactive]="!item.isActive">{{ item.isActive ? 'Active' : 'Inactive' }}</span></td>
                <td class="row-actions">
                  <button mat-icon-button type="button" aria-label="Edit" (click)="edit(item)">
                    <mat-icon aria-hidden="true">edit</mat-icon>
                  </button>
                  <button mat-icon-button type="button" aria-label="Deactivate" [disabled]="!item.isActive" (click)="deactivate(item)">
                    <mat-icon aria-hidden="true">block</mat-icon>
                  </button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="empty">No records found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `,
  styles: [`
    .manager { display: grid; gap: 1rem; }
    header { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
    h1 { margin: 0; color: #111827; font-size: 1.65rem; line-height: 1.2; }
    .eyebrow { margin: 0 0 0.2rem; color: #5b6475; font-size: 0.8rem; font-weight: 700; text-transform: uppercase; }
    form { display: grid; max-width: 760px; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 0.75rem; }
    textarea { resize: vertical; }
    .actions { display: flex; align-items: center; gap: 0.65rem; grid-column: 1 / -1; }
    .table-wrap { overflow: auto; border: 1px solid #d8dee8; border-radius: 8px; background: #fff; }
    table { width: 100%; min-width: 760px; border-collapse: collapse; }
    th, td { padding: 0.8rem 0.9rem; border-bottom: 1px solid #e8edf5; text-align: left; vertical-align: middle; }
    th { color: #3d4758; font-size: 0.78rem; font-weight: 700; text-transform: uppercase; }
    tr:last-child td { border-bottom: 0; }
    .status { display: inline-block; min-width: 4.6rem; border-radius: 999px; padding: 0.2rem 0.55rem; background: #e8f6ef; color: #166534; text-align: center; font-size: 0.78rem; font-weight: 700; }
    .status.inactive { background: #f2f4f7; color: #5b6475; }
    .row-actions { width: 7rem; white-space: nowrap; }
    .error, .empty { color: #b91c1c; }
    @media (max-width: 720px) { form { grid-template-columns: 1fr; } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrganizationEntityManagerComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly organization = inject(OrganizationService);
  private readonly errorParser = inject(ApiErrorParserService);

  protected readonly kind = signal<EntityKind>((this.route.snapshot.data['kind'] as EntityKind) ?? 'regions');
  protected readonly entities = signal<OrganizationEntity[]>([]);
  protected readonly regions = signal<Region[]>([]);
  protected readonly countries = signal<Country[]>([]);
  protected readonly accounts = signal<Account[]>([]);
  protected readonly errorMessage = signal('');
  protected readonly editingId = signal<string | null>(null);
  protected readonly isSaving = signal(false);

  protected readonly title = computed(() => {
    const labels: Record<EntityKind, string> = { regions: 'Regions', countries: 'Countries', accounts: 'Accounts', campaigns: 'Campaigns' };
    return labels[this.kind()];
  });
  protected readonly activeRegions = computed(() => this.regions().filter((r) => r.isActive));
  protected readonly activeCountries = computed(() => this.countries().filter((c) => c.isActive));
  protected readonly activeAccounts = computed(() => this.accounts().filter((a) => a.isActive));

  protected readonly form = this.formBuilder.nonNullable.group({
    regionId: [''],
    countryId: [''],
    accountId: [''],
    code: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(500)]]
  });

  constructor() {
    this.configureValidators();
    this.load();
  }

  protected parentLabel(item: OrganizationEntity): string {
    if ('accountCode' in item) return `${item.accountCode} / ${item.countryCode}`;
    if ('countryCode' in item) return item.countryCode;
    if ('regionCode' in item) return item.regionCode;
    return 'Root';
  }

  protected edit(item: OrganizationEntity): void {
    this.editingId.set(item.id);
    this.form.patchValue({
      regionId: 'regionId' in item ? item.regionId : '',
      countryId: 'countryId' in item ? item.countryId : '',
      accountId: 'accountId' in item ? item.accountId : '',
      code: item.code,
      name: item.name,
      description: 'description' in item ? item.description ?? '' : ''
    });
  }

  protected resetForm(): void {
    this.editingId.set(null);
    this.form.reset({ regionId: '', countryId: '', accountId: '', code: '', name: '', description: '' });
  }

  protected syncCampaignCountry(): void {
    const account = this.accounts().find((candidate) => candidate.id === this.form.controls.accountId.value);
    if (account) this.form.controls.countryId.setValue(account.countryId);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.isSaving.set(true);
    const raw = this.form.getRawValue();
    const base = { code: raw.code.trim(), name: raw.name.trim() };
    const id = this.editingId();
    const request$ = this.kind() === 'regions'
      ? (id ? this.organization.updateRegion(id, base) : this.organization.createRegion(base))
      : this.kind() === 'countries'
        ? (id ? this.organization.updateCountry(id, { ...base, regionId: raw.regionId }) : this.organization.createCountry({ ...base, regionId: raw.regionId }))
        : this.kind() === 'accounts'
          ? (id ? this.organization.updateAccount(id, { ...base, countryId: raw.countryId, description: raw.description.trim() || null }) : this.organization.createAccount({ ...base, countryId: raw.countryId, description: raw.description.trim() || null }))
          : (id ? this.organization.updateCampaign(id, { ...base, accountId: raw.accountId, countryId: raw.countryId, description: raw.description.trim() || null }) : this.organization.createCampaign({ ...base, accountId: raw.accountId, countryId: raw.countryId, description: raw.description.trim() || null }));

    request$.subscribe({
      next: () => {
        this.resetForm();
        this.isSaving.set(false);
        this.load();
      },
      error: (error) => {
        this.errorMessage.set(this.errorParser.parse(error).message);
        this.isSaving.set(false);
      }
    });
  }

  protected deactivate(item: OrganizationEntity): void {
    this.organization.deactivate(this.kind(), item.id).subscribe({
      next: () => this.load(),
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }

  private configureValidators(): void {
    if (this.kind() === 'countries') this.form.controls.regionId.addValidators(Validators.required);
    if (this.kind() === 'accounts') this.form.controls.countryId.addValidators(Validators.required);
    if (this.kind() === 'campaigns') {
      this.form.controls.accountId.addValidators(Validators.required);
      this.form.controls.countryId.addValidators(Validators.required);
    }
  }

  private load(): void {
    forkJoin({
      regions: this.organization.getRegions(),
      countries: this.organization.getCountries(),
      accounts: this.organization.getAccounts(),
      campaigns: this.organization.getCampaigns()
    }).subscribe({
      next: ({ regions, countries, accounts, campaigns }) => {
        this.regions.set(regions);
        this.countries.set(countries);
        this.accounts.set(accounts);
        this.entities.set(this.kind() === 'regions' ? regions : this.kind() === 'countries' ? countries : this.kind() === 'accounts' ? accounts : campaigns);
      },
      error: (error) => this.errorMessage.set(this.errorParser.parse(error).message)
    });
  }
}
