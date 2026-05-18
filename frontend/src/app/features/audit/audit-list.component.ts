import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { SafeApiError } from '../../core/models/api-error.models';
import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { AuditLogDetail, AuditLogFilters, AuditLogListItem } from './audit.models';
import { AuditService } from './audit.service';

@Component({
  selector: 'app-audit-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Audit Logs</h1>
      </div>

      <form class="filter-panel" [formGroup]="filterForm" (ngSubmit)="applyFilters()">
        <div class="form-grid">
          <div class="form-group">
            <label for="actorUserId">Actor User ID</label>
            <input id="actorUserId" type="text" formControlName="actorUserId" />
          </div>
          <div class="form-group">
            <label for="action">Action</label>
            <input id="action" type="text" formControlName="action" />
          </div>
          <div class="form-group">
            <label for="entityType">Entity Type</label>
            <input id="entityType" type="text" formControlName="entityType" />
          </div>
          <div class="form-group">
            <label for="entityId">Entity ID</label>
            <input id="entityId" type="text" formControlName="entityId" />
          </div>
          <div class="form-group">
            <label for="fromUtc">From</label>
            <input id="fromUtc" type="datetime-local" formControlName="fromUtc" />
          </div>
          <div class="form-group">
            <label for="toUtc">To</label>
            <input id="toUtc" type="datetime-local" formControlName="toUtc" />
          </div>
        </div>

        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">Apply Filters</button>
          <button type="button" class="btn btn-secondary" (click)="resetFilters()" [disabled]="loading">Reset</button>
        </div>
      </form>

      <div *ngIf="loading" class="loading">Loading audit logs...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <table *ngIf="!loading && auditLogs.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Created</th>
            <th>Actor</th>
            <th>Action</th>
            <th>Entity Type</th>
            <th>Entity ID</th>
            <th>Details</th>
          </tr>
        </thead>
        <tbody>
          <ng-container *ngFor="let item of auditLogs">
            <tr>
              <td>{{ item.createdAt | date:'medium' }}</td>
              <td>{{ item.actorDisplayName || item.actorUserId || 'System' }}</td>
              <td>{{ item.action }}</td>
              <td>{{ item.entityType }}</td>
              <td>{{ item.entityId }}</td>
              <td>
                <button type="button" class="btn btn-secondary" (click)="toggleDetail(item)" [disabled]="detailLoadingId === item.id">
                  {{ expandedId === item.id ? 'Hide' : 'View' }}
                </button>
              </td>
            </tr>
            <tr *ngIf="expandedId === item.id">
              <td colspan="6">
                <div *ngIf="detailLoadingId === item.id" class="loading">Loading details...</div>
                <div *ngIf="detailError" class="error">{{ detailError }}</div>
                <div *ngIf="selectedDetail" class="audit-detail">
                  <p><strong>Correlation ID:</strong> {{ selectedDetail.correlationId || 'Not set' }}</p>
                  <div class="detail-grid">
                    <div>
                      <h2>Previous Value</h2>
                      <pre>{{ formatAuditValue(selectedDetail.previousValue) }}</pre>
                    </div>
                    <div>
                      <h2>New Value</h2>
                      <pre>{{ formatAuditValue(selectedDetail.newValue) }}</pre>
                    </div>
                  </div>
                </div>
              </td>
            </tr>
          </ng-container>
        </tbody>
      </table>

      <p *ngIf="!loading && auditLogs.length === 0">No audit logs found.</p>

      <div class="pagination">
        <button type="button" class="btn btn-secondary" (click)="goToPage(page - 1)" [disabled]="loading || page <= 1">Previous</button>
        <span>Page {{ page }} of {{ totalPages || 1 }}</span>
        <button type="button" class="btn btn-secondary" (click)="goToPage(page + 1)" [disabled]="loading || page >= totalPages">Next</button>
        <label for="pageSize">Rows</label>
        <select id="pageSize" [value]="pageSize" (change)="changePageSize($event)">
          <option value="10">10</option>
          <option value="25">25</option>
          <option value="50">50</option>
          <option value="100">100</option>
        </select>
      </div>
    </div>
  `,
  styles: [`
    .filter-panel { margin-bottom: 1rem; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; }
    .actions, .pagination { display: flex; align-items: center; gap: .75rem; flex-wrap: wrap; margin: 1rem 0; }
    .audit-detail { padding: .75rem 0; }
    .detail-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; }
    .detail-grid h2 { font-size: 1rem; margin: 0 0 .5rem; }
    pre { white-space: pre-wrap; word-break: break-word; margin: 0; }
  `]
})
export class AuditListComponent implements OnInit {
  private readonly auditService = inject(AuditService);
  private readonly errorParser = inject(ApiErrorParserService);
  private readonly fb = inject(FormBuilder);

  auditLogs: AuditLogListItem[] = [];
  selectedDetail: AuditLogDetail | null = null;
  expandedId: string | null = null;
  detailLoadingId: string | null = null;
  loading = true;
  error: string | null = null;
  detailError: string | null = null;
  page = 1;
  pageSize = 25;
  totalPages = 0;
  totalCount = 0;

  filterForm = this.fb.group({
    actorUserId: [''],
    action: [''],
    entityType: [''],
    entityId: [''],
    fromUtc: [''],
    toUtc: ['']
  });

  ngOnInit() {
    this.loadAuditLogs();
  }

  applyFilters() {
    this.page = 1;
    this.loadAuditLogs();
  }

  resetFilters() {
    this.filterForm.reset({
      actorUserId: '',
      action: '',
      entityType: '',
      entityId: '',
      fromUtc: '',
      toUtc: ''
    });
    this.page = 1;
    this.loadAuditLogs();
  }

  goToPage(page: number) {
    if (page < 1 || (this.totalPages > 0 && page > this.totalPages)) return;
    this.page = page;
    this.loadAuditLogs();
  }

  changePageSize(event: Event) {
    const value = Number((event.target as HTMLSelectElement).value);
    this.pageSize = value;
    this.page = 1;
    this.loadAuditLogs();
  }

  toggleDetail(item: AuditLogListItem) {
    if (this.expandedId === item.id) {
      this.expandedId = null;
      this.selectedDetail = null;
      this.detailError = null;
      return;
    }

    this.expandedId = item.id;
    this.selectedDetail = null;
    this.detailError = null;
    this.detailLoadingId = item.id;

    this.auditService.getAuditLogById(item.id).subscribe({
      next: (detail) => {
        this.selectedDetail = detail;
        this.detailLoadingId = null;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.detailError = parsed.message;
        this.detailLoadingId = null;
      }
    });
  }

  formatAuditValue(value?: string | null) {
    if (!value) return 'Not set';

    try {
      return JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      return value;
    }
  }

  private loadAuditLogs() {
    this.loading = true;
    this.error = null;
    this.expandedId = null;
    this.selectedDetail = null;
    this.detailError = null;

    this.auditService.getAuditLogs(this.buildFilters()).subscribe({
      next: (result) => {
        this.auditLogs = result.items;
        this.page = result.page;
        this.pageSize = result.pageSize;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.loading = false;
      },
      error: (err) => {
        const parsed: SafeApiError = this.errorParser.parse(err);
        this.error = parsed.message;
        this.auditLogs = [];
        this.loading = false;
      }
    });
  }

  private buildFilters(): AuditLogFilters {
    const value = this.filterForm.getRawValue();
    return {
      actorUserId: this.clean(value.actorUserId),
      action: this.clean(value.action),
      entityType: this.clean(value.entityType),
      entityId: this.clean(value.entityId),
      fromUtc: this.clean(value.fromUtc),
      toUtc: this.clean(value.toUtc),
      page: this.page,
      pageSize: this.pageSize
    };
  }

  private clean(value?: string | null) {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }
}
