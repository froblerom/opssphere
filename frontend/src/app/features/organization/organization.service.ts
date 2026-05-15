import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import {
  Account,
  AccountRequest,
  Campaign,
  CampaignRequest,
  Country,
  CountryRequest,
  EntityKind,
  ListResponse,
  Region,
  RegionRequest,
  UpdateUserScopesRequest,
  UserScopeAssignment
} from './organization.models';

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {
  private readonly apiClient = inject(ApiClientService);

  getRegions() {
    return this.apiClient.get<ListResponse<Region>>('regions').pipe(map((response) => response.data));
  }

  getCountries() {
    return this.apiClient.get<ListResponse<Country>>('countries').pipe(map((response) => response.data));
  }

  getAccounts() {
    return this.apiClient.get<ListResponse<Account>>('accounts').pipe(map((response) => response.data));
  }

  getCampaigns() {
    return this.apiClient.get<ListResponse<Campaign>>('campaigns').pipe(map((response) => response.data));
  }

  createRegion(request: RegionRequest) {
    return this.apiClient.post<RegionRequest, ApiResponse<Region>>('regions', request).pipe(map((response) => response.data));
  }

  updateRegion(id: string, request: RegionRequest) {
    return this.apiClient.put<RegionRequest, ApiResponse<Region>>(`regions/${id}`, request).pipe(map((response) => response.data));
  }

  createCountry(request: CountryRequest) {
    return this.apiClient.post<CountryRequest, ApiResponse<Country>>('countries', request).pipe(map((response) => response.data));
  }

  updateCountry(id: string, request: CountryRequest) {
    return this.apiClient.put<CountryRequest, ApiResponse<Country>>(`countries/${id}`, request).pipe(map((response) => response.data));
  }

  createAccount(request: AccountRequest) {
    return this.apiClient.post<AccountRequest, ApiResponse<Account>>('accounts', request).pipe(map((response) => response.data));
  }

  updateAccount(id: string, request: AccountRequest) {
    return this.apiClient.put<AccountRequest, ApiResponse<Account>>(`accounts/${id}`, request).pipe(map((response) => response.data));
  }

  createCampaign(request: CampaignRequest) {
    return this.apiClient.post<CampaignRequest, ApiResponse<Campaign>>('campaigns', request).pipe(map((response) => response.data));
  }

  updateCampaign(id: string, request: CampaignRequest) {
    return this.apiClient.put<CampaignRequest, ApiResponse<Campaign>>(`campaigns/${id}`, request).pipe(map((response) => response.data));
  }

  deactivate(kind: EntityKind, id: string) {
    return this.apiClient.post<Record<string, never>, unknown>(`${kind}/${id}/deactivate`, {});
  }

  getUserScopes(userId: string) {
    return this.apiClient.get<ApiResponse<UserScopeAssignment>>(`users/${userId}/scopes`).pipe(map((response) => response.data));
  }

  updateUserScopes(userId: string, request: UpdateUserScopesRequest) {
    return this.apiClient.put<UpdateUserScopesRequest, ApiResponse<UserScopeAssignment>>(`users/${userId}/scopes`, request).pipe(map((response) => response.data));
  }
}
