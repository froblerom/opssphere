import { ApiResponse } from '../../core/auth/auth.models';

export type ListResponse<T> = ApiResponse<T[]>;

export interface RoleSummary {
  id: string;
  name: string;
  description?: string | null;
  isSystemRole: boolean;
  isActive: boolean;
}

export interface PermissionSummary {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
}

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
  lastLoginAt?: string | null;
  roles: RoleSummary[];
}

export interface UserDetail extends UserSummary {
  deactivatedAt?: string | null;
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  temporaryPassword: string;
}

export interface UpdateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
}

export interface UpdateUserRolesRequest {
  roleIds: string[];
}
