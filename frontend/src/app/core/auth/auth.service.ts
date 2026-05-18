import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';

import { ApiClientService } from '../services/api-client.service';
import { ApiResponse, AuthUserSummary, CurrentUserProfile, LoginRequest, LoginResponse } from './auth.models';
import { TokenStorageService } from './token-storage.service';

// SECURITY NOTE: Frontend visibility is UX only. Backend authorization is the source of truth.
// hasRole/hasPermission helpers improve UX by hiding unavailable actions.
// They are NOT security controls - backend enforces authorization on every request.

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiClient = inject(ApiClientService);
  private readonly tokenStorage = inject(TokenStorageService);

  readonly currentUser = signal<AuthUserSummary | null>(null);
  readonly currentProfile = signal<CurrentUserProfile | null>(null);
  readonly isAuthenticated = computed(() => Boolean(this.tokenStorage.token()));

  constructor() {
    effect(() => {
      if (!this.tokenStorage.token()) {
        this.currentUser.set(null);
        this.currentProfile.set(null);
      }
    });
  }

  login(email: string, password: string) {
    const request: LoginRequest = { email, password };

    return this.apiClient.post<LoginRequest, ApiResponse<LoginResponse>>('auth/login', request).pipe(
      map((response) => response.data),
      tap((loginResponse) => {
        this.tokenStorage.setToken(loginResponse.accessToken);
        this.currentUser.set(loginResponse.user);
      })
    );
  }

  me() {
    return this.apiClient.get<ApiResponse<CurrentUserProfile>>('auth/me').pipe(
      map((response) => response.data),
      tap((profile) => {
        this.currentProfile.set(profile);
        this.currentUser.set({
          id: profile.id,
          email: profile.email,
          displayName: profile.displayName,
          roles: profile.roles
        });
      })
    );
  }

  logout(): void {
    this.tokenStorage.clearToken();
    this.currentUser.set(null);
    this.currentProfile.set(null);
  }

  getAccessToken(): string | null {
    const token = this.tokenStorage.getUsableToken();
    if (!token) {
      this.logout();
      return null;
    }

    return token;
  }

  // UX helper - NOT a security control. Backend enforces roles on every request.
  hasRole(role: string): boolean {
    return this.currentUser()?.roles.includes(role) ?? false;
  }

  // UX helper - NOT a security control. Backend enforces roles on every request.
  hasAnyRole(roles: string[]): boolean {
    const userRoles = this.currentUser()?.roles;
    if (!userRoles) return false;
    return roles.some((role) => userRoles.includes(role));
  }

  // UX helper - NOT a security control. Backend enforces permissions on every request.
  hasPermission(permission: string): boolean {
    return this.currentProfile()?.permissions.includes(permission) ?? false;
  }

  // UX helper - NOT a security control. Backend enforces permissions on every request.
  hasAnyPermission(permissions: string[]): boolean {
    const userPermissions = this.currentProfile()?.permissions;
    if (!userPermissions) return false;
    return permissions.some((p) => userPermissions.includes(p));
  }
}
