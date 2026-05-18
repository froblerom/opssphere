import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import { AuditLogDetail, AuditLogFilters, AuditLogListItem, PagedApiResponse, PagedResult } from './audit.models';

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private readonly apiClient = inject(ApiClientService);

  getAuditLogs(filters: AuditLogFilters = {}) {
    return this.apiClient
      .get<PagedApiResponse<AuditLogListItem>>(`audit-logs${this.buildQuery(filters)}`)
      .pipe(map((response) => this.unwrapPaged(response)));
  }

  getAuditLogById(id: string) {
    return this.apiClient
      .get<ApiResponse<AuditLogDetail>>(`audit-logs/${id}`)
      .pipe(map((response) => response.data));
  }

  getEntityAuditHistory(entityType: string, entityId: string, filters: Pick<AuditLogFilters, 'fromUtc' | 'toUtc' | 'page' | 'pageSize'> = {}) {
    return this.apiClient
      .get<PagedApiResponse<AuditLogListItem>>(
        `audit-logs/entity/${encodeURIComponent(entityType)}/${entityId}${this.buildQuery(filters)}`
      )
      .pipe(map((response) => this.unwrapPaged(response)));
  }

  private buildQuery(filters: AuditLogFilters) {
    const params = new URLSearchParams();
    if (filters.actorUserId) params.set('actorUserId', filters.actorUserId);
    if (filters.action) params.set('action', filters.action);
    if (filters.entityType) params.set('entityType', filters.entityType);
    if (filters.entityId) params.set('entityId', filters.entityId);
    if (filters.fromUtc) params.set('fromUtc', filters.fromUtc);
    if (filters.toUtc) params.set('toUtc', filters.toUtc);
    if (filters.accountId) params.set('accountId', filters.accountId);
    if (filters.campaignId) params.set('campaignId', filters.campaignId);
    if (filters.page) params.set('page', String(filters.page));
    if (filters.pageSize) params.set('pageSize', String(filters.pageSize));

    const query = params.toString();
    return query ? `?${query}` : '';
  }

  private unwrapPaged<T>(response: PagedApiResponse<T>): PagedResult<T> {
    return {
      items: response.data,
      page: response.page,
      pageSize: response.pageSize,
      totalCount: response.totalCount,
      totalPages: response.totalPages
    };
  }
}
