import { computed, inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';

import { ApiClientService } from '../services/api-client.service';
import { ApiResponse, AuthUserSummary, CurrentUserProfile, LoginRequest, LoginResponse } from './auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiClient = inject(ApiClientService);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly currentToken = signal(this.tokenStorage.getUsableToken());

  readonly currentUser = signal<AuthUserSummary | null>(null);
  readonly isAuthenticated = computed(() => Boolean(this.currentToken()));

  login(email: string, password: string) {
    const request: LoginRequest = { email, password };

    return this.apiClient.post<LoginRequest, ApiResponse<LoginResponse>>('auth/login', request).pipe(
      map((response) => response.data),
      tap((loginResponse) => {
        this.tokenStorage.setToken(loginResponse.accessToken);
        this.currentToken.set(loginResponse.accessToken);
        this.currentUser.set(loginResponse.user);
      })
    );
  }

  me() {
    return this.apiClient.get<ApiResponse<CurrentUserProfile>>('auth/me').pipe(
      map((response) => response.data),
      tap((profile) => {
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
    this.currentToken.set(null);
    this.currentUser.set(null);
  }

  getAccessToken(): string | null {
    const token = this.currentToken() ?? this.tokenStorage.getUsableToken();
    if (!token) {
      this.logout();
      return null;
    }

    return token;
  }
}
