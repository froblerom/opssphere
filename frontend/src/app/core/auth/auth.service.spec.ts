import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { AppPermissions, AppRoles } from './auth-permissions';
import { CurrentUserProfile } from './auth.models';
import { TokenStorageService } from './token-storage.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTesting: HttpTestingController;
  let tokenStorage: TokenStorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(AuthService);
    httpTesting = TestBed.inject(HttpTestingController);
    tokenStorage = TestBed.inject(TokenStorageService);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  it('sends login request and stores token', (done) => {
    const token = createToken();

    service.login('agent.novabank@opssphere.local', 'OpsSphere123!').subscribe((response) => {
      expect(response.accessToken).toBe(token);
      expect(tokenStorage.getToken()).toBe(token);
      expect(service.isAuthenticated()).toBeTrue();
      done();
    });

    const request = httpTesting.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'agent.novabank@opssphere.local',
      password: 'OpsSphere123!'
    });
    request.flush({
      data: {
        accessToken: token,
        tokenType: 'Bearer',
        expiresIn: 3600,
        user: {
          id: '00000000-0000-0000-0000-000000000001',
          email: 'agent.novabank@opssphere.local',
          displayName: 'Agent NovaBank',
          roles: ['Agent']
        }
      }
    });
  });

  it('clears token on logout', () => {
    tokenStorage.setToken(createToken());

    service.logout();

    expect(tokenStorage.getToken()).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentProfile()).toBeNull();
  });

  // hasRole — UX helper, not a security control
  it('hasRole returns true when user has the role', () => {
    setAgentProfile(service);
    expect(service.hasRole(AppRoles.Agent)).toBeTrue();
  });

  it('hasRole returns false when user does not have the role', () => {
    setAgentProfile(service);
    expect(service.hasRole(AppRoles.Admin)).toBeFalse();
  });

  it('hasRole returns false when not authenticated', () => {
    expect(service.hasRole(AppRoles.Admin)).toBeFalse();
  });

  // hasPermission — UX helper, not a security control
  it('hasPermission returns true when profile has the permission', () => {
    setViewerProfile(service);
    expect(service.hasPermission(AppPermissions.DashboardView)).toBeTrue();
  });

  it('hasPermission returns false when profile lacks the permission', () => {
    setViewerProfile(service);
    expect(service.hasPermission(AppPermissions.TicketsCreate)).toBeFalse();
  });

  it('hasPermission returns false when profile is null', () => {
    expect(service.hasPermission(AppPermissions.DashboardView)).toBeFalse();
  });

  // hasAnyPermission — UX helper, not a security control
  it('hasAnyPermission returns true when at least one permission matches', () => {
    setViewerProfile(service);
    expect(service.hasAnyPermission([AppPermissions.TicketsCreate, AppPermissions.DashboardView])).toBeTrue();
  });

  it('hasAnyPermission returns false when no permission matches', () => {
    setViewerProfile(service);
    expect(service.hasAnyPermission([AppPermissions.TicketsCreate, AppPermissions.UsersManage])).toBeFalse();
  });

  it('hasAnyPermission returns false when profile is null', () => {
    expect(service.hasAnyPermission([AppPermissions.DashboardView])).toBeFalse();
  });

  // Navigation visibility — UX only, backend authorization is the source of truth
  it('Admin role shows admin nav, Agent role does not', () => {
    setViewerProfile(service);
    expect(service.hasRole(AppRoles.Admin)).toBeFalse();

    setAgentProfile(service);
    expect(service.hasRole(AppRoles.Admin)).toBeFalse();
  });

  it('Admin user would see admin nav via hasRole', () => {
    setAdminProfile(service);
    expect(service.hasRole(AppRoles.Admin)).toBeTrue();
  });

  it('Viewer does not have write permissions', () => {
    setViewerProfile(service);
    expect(service.hasPermission(AppPermissions.TicketsCreate)).toBeFalse();
    expect(service.hasPermission(AppPermissions.UsersManage)).toBeFalse();
  });
});

// Helpers to set up signals directly for unit test isolation
function setAgentProfile(service: AuthService): void {
  const profile = buildProfile('Agent', ['tickets.view', 'tickets.create', 'dashboard.view']);
  service.currentUser.set({ id: 'agent-id', email: 'agent@test.local', displayName: 'Test Agent', roles: ['Agent'] });
  service.currentProfile.set(profile);
}

function setViewerProfile(service: AuthService): void {
  const profile = buildProfile('Viewer', ['dashboard.view', 'tickets.view', 'reports.view', 'audit.view']);
  service.currentUser.set({ id: 'viewer-id', email: 'viewer@test.local', displayName: 'Test Viewer', roles: ['Viewer'] });
  service.currentProfile.set(profile);
}

function setAdminProfile(service: AuthService): void {
  const profile = buildProfile('Admin', ['users.manage', 'roles.manage', 'dashboard.view', 'audit.admin_view']);
  service.currentUser.set({ id: 'admin-id', email: 'admin@test.local', displayName: 'Test Admin', roles: ['Admin'] });
  service.currentProfile.set(profile);
}

function buildProfile(role: string, permissions: string[]): CurrentUserProfile {
  return {
    id: 'test-id',
    email: 'test@test.local',
    displayName: 'Test User',
    roles: [role],
    permissions,
    scopes: []
  };
}

function createToken(expOffsetSeconds = 3600): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  const encodedPayload = btoa(JSON.stringify(payload)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

  return `header.${encodedPayload}.signature`;
}
