import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly tokenKey = 'opssphere.accessToken';

  getToken(): string | null {
    return this.storage()?.getItem(this.tokenKey) ?? null;
  }

  getUsableToken(): string | null {
    const token = this.getToken();
    if (!this.isUsableToken(token)) {
      this.clearToken();
      return null;
    }

    return token;
  }

  setToken(token: string): void {
    this.storage()?.setItem(this.tokenKey, token);
  }

  clearToken(): void {
    this.storage()?.removeItem(this.tokenKey);
  }

  private storage(): Storage | null {
    return typeof localStorage === 'undefined' ? null : localStorage;
  }

  private isUsableToken(token: string | null): boolean {
    if (!token) {
      return false;
    }

    const expiresAt = this.getTokenExpiration(token);
    return expiresAt !== null && expiresAt > Date.now();
  }

  private getTokenExpiration(token: string): number | null {
    const payload = token.split('.')[1];
    if (!payload) {
      return null;
    }

    try {
      const paddedPayload = payload.padEnd(payload.length + ((4 - (payload.length % 4)) % 4), '=');
      const normalizedPayload = paddedPayload.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = JSON.parse(atob(normalizedPayload)) as { exp?: number };
      return typeof jsonPayload.exp === 'number' ? jsonPayload.exp * 1000 : null;
    } catch {
      return null;
    }
  }
}
