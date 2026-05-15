import { ApiResponse } from '../../core/auth/auth.models';

export type ListResponse<T> = ApiResponse<T[]>;
export type EntityKind = 'regions' | 'countries' | 'accounts' | 'campaigns';

export interface Region {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface Country extends Region {
  regionId: string;
  regionCode: string;
  regionName: string;
}

export interface Account extends Region {
  countryId: string;
  countryCode: string;
  countryName: string;
  regionId: string;
  regionCode: string;
  description?: string | null;
}

export interface Campaign extends Region {
  accountId: string;
  accountCode: string;
  accountName: string;
  countryId: string;
  countryCode: string;
  countryName: string;
  regionId: string;
  regionCode: string;
  description?: string | null;
}

export type OrganizationEntity = Region | Country | Account | Campaign;

export interface RegionRequest {
  code: string;
  name: string;
}

export interface CountryRequest extends RegionRequest {
  regionId: string;
}

export interface AccountRequest extends RegionRequest {
  countryId: string;
  description?: string | null;
}

export interface CampaignRequest extends RegionRequest {
  accountId: string;
  countryId: string;
  description?: string | null;
}

export interface ScopeRequest {
  scopeType: string;
  regionId?: string | null;
  countryId?: string | null;
  accountId?: string | null;
  campaignId?: string | null;
}

export interface UserScope extends ScopeRequest {
  id: string;
  regionCode?: string | null;
  regionName?: string | null;
  countryCode?: string | null;
  countryName?: string | null;
  accountCode?: string | null;
  accountName?: string | null;
  campaignCode?: string | null;
  campaignName?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface UserScopeAssignment {
  userId: string;
  email: string;
  displayName: string;
  isActive: boolean;
  roles: string[];
  scopes: UserScope[];
}

export interface UpdateUserScopesRequest {
  scopes: ScopeRequest[];
}
