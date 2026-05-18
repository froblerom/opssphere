import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import { DashboardFilters, OperationalDashboard } from './dashboard.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiClient = inject(ApiClientService);

  getOperationalDashboard(filters: DashboardFilters = {}) {
    return this.apiClient
      .get<ApiResponse<OperationalDashboard>>(`dashboard/operational${this.buildQuery(filters)}`)
      .pipe(map((response) => response.data));
  }

  private buildQuery(filters: DashboardFilters) {
    const params = new URLSearchParams();
    if (filters.regionId) params.set('regionId', filters.regionId);
    if (filters.countryId) params.set('countryId', filters.countryId);
    if (filters.accountId) params.set('accountId', filters.accountId);
    if (filters.campaignId) params.set('campaignId', filters.campaignId);
    if (filters.supervisorUserId) params.set('supervisorUserId', filters.supervisorUserId);
    if (filters.agentUserId) params.set('agentUserId', filters.agentUserId);
    if (filters.status) params.set('status', filters.status);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.slaState) params.set('slaState', filters.slaState);
    if (filters.dateFrom) params.set('dateFrom', filters.dateFrom);
    if (filters.dateTo) params.set('dateTo', filters.dateTo);

    const query = params.toString();
    return query ? `?${query}` : '';
  }
}
