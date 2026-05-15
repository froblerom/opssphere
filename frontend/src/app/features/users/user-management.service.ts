import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import {
  CreateUserRequest,
  ListResponse,
  PermissionSummary,
  RoleSummary,
  UpdateUserRequest,
  UpdateUserRolesRequest,
  UserDetail,
  UserSummary
} from './user-management.models';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private readonly apiClient = inject(ApiClientService);

  getUsers() {
    return this.apiClient.get<ListResponse<UserSummary>>('users').pipe(map((response) => response.data));
  }

  getUser(id: string) {
    return this.apiClient.get<ApiResponse<UserDetail>>(`users/${id}`).pipe(map((response) => response.data));
  }

  createUser(request: CreateUserRequest) {
    return this.apiClient.post<CreateUserRequest, ApiResponse<UserDetail>>('users', request).pipe(map((response) => response.data));
  }

  updateUser(id: string, request: UpdateUserRequest) {
    return this.apiClient.put<UpdateUserRequest, ApiResponse<UserDetail>>(`users/${id}`, request).pipe(map((response) => response.data));
  }

  deactivateUser(id: string) {
    return this.apiClient.post<Record<string, never>, unknown>(`users/${id}/deactivate`, {});
  }

  updateUserRoles(id: string, roleIds: string[]) {
    const request: UpdateUserRolesRequest = { roleIds };
    return this.apiClient.put<UpdateUserRolesRequest, ApiResponse<UserDetail>>(`users/${id}/roles`, request).pipe(map((response) => response.data));
  }

  getRoles() {
    return this.apiClient.get<ListResponse<RoleSummary>>('roles').pipe(map((response) => response.data));
  }

  getPermissions() {
    return this.apiClient.get<ListResponse<PermissionSummary>>('permissions').pipe(map((response) => response.data));
  }
}
