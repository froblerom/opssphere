export interface ApiResponse<T> {
  data: T;
}

export interface ApiErrorResponse {
  error: {
    code: string;
    message: string;
  };
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: 'Bearer';
  expiresIn: number;
  user: AuthUserSummary;
}

export interface AuthUserSummary {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
}

export interface CurrentUserProfile extends AuthUserSummary {
  permissions: string[];
  scopes: UserScope[];
}

export interface UserScope {
  scopeType: string;
  regionId?: string | null;
  regionCode?: string | null;
  countryId?: string | null;
  countryCode?: string | null;
  accountId?: string | null;
  accountCode?: string | null;
  campaignId?: string | null;
  campaignCode?: string | null;
}
