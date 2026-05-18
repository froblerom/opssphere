import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../auth/auth.service';
import { authGuard, permissionGuard } from './auth.guard';

describe('authGuard', () => {
  let authService: jasmine.SpyObj<Pick<AuthService, 'getAccessToken'>>;

  beforeEach(() => {
    authService = jasmine.createSpyObj<Pick<AuthService, 'getAccessToken'>>('AuthService', ['getAccessToken']);
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService }
      ]
    });
  });

  it('allows authenticated navigation', () => {
    authService.getAccessToken.and.returnValue('jwt-token');

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/dashboard' } as RouterStateSnapshot)
    );

    expect(result).toBeTrue();
  });

  it('redirects unauthenticated users to login', () => {
    authService.getAccessToken.and.returnValue(null);
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/dashboard' } as RouterStateSnapshot)
    );

    expect(router.serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/login?returnUrl=%2Fdashboard');
  });
});

describe('permissionGuard', () => {
  let authService: jasmine.SpyObj<Pick<AuthService, 'getAccessToken' | 'hasAnyPermission' | 'hasAnyRole' | 'me'>> & {
    currentProfile: jasmine.Spy;
  };

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', [
      'getAccessToken',
      'hasAnyPermission',
      'hasAnyRole',
      'me',
      'currentProfile'
    ]);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService }
      ]
    });
  });

  it('redirects unauthenticated users to login with returnUrl', () => {
    authService.getAccessToken.and.returnValue(null);
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(routeWithData({ permissions: ['users.view'], roles: ['Admin'] }), { url: '/admin/users' } as RouterStateSnapshot)
    );

    expect(router.serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/login?returnUrl=%2Fadmin%2Fusers');
  });

  it('allows users with the required role and permission', () => {
    authService.getAccessToken.and.returnValue('jwt-token');
    authService.currentProfile.and.returnValue({ id: 'user-1' });
    authService.hasAnyPermission.and.returnValue(true);
    authService.hasAnyRole.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(routeWithData({ permissions: ['users.manage'], roles: ['Admin'] }), { url: '/admin/users/create' } as RouterStateSnapshot)
    );

    expect(result).toBeTrue();
  });

  it('redirects authenticated users without permission to access denied', () => {
    authService.getAccessToken.and.returnValue('jwt-token');
    authService.currentProfile.and.returnValue({ id: 'user-1' });
    authService.hasAnyPermission.and.returnValue(false);
    authService.hasAnyRole.and.returnValue(true);
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(routeWithData({ permissions: ['users.manage'], roles: ['Admin'] }), { url: '/admin/users/create' } as RouterStateSnapshot)
    );

    expect(router.serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe('/access-denied');
  });

  it('loads the profile before evaluating permissions when needed', (done) => {
    authService.getAccessToken.and.returnValue('jwt-token');
    authService.currentProfile.and.returnValue(null);
    authService.me.and.returnValue(of({
      id: 'user-1',
      email: 'admin@opssphere.test',
      displayName: 'Admin User',
      roles: ['Admin'],
      permissions: ['roles.manage'],
      scopes: []
    }));
    authService.hasAnyPermission.and.returnValue(true);
    authService.hasAnyRole.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(routeWithData({ permissions: ['roles.manage'], roles: ['Admin'] }), { url: '/admin/users/user-1/roles' } as RouterStateSnapshot)
    );

    (result as any).subscribe((value: unknown) => {
      expect(value).toBeTrue();
      expect(authService.me).toHaveBeenCalled();
      done();
    });
  });

  it('redirects to login when profile loading fails', (done) => {
    authService.getAccessToken.and.returnValue('jwt-token');
    authService.currentProfile.and.returnValue(null);
    authService.me.and.returnValue(throwError(() => new Error('expired token')));
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(routeWithData({ permissions: ['users.view'], roles: ['Admin'] }), { url: '/admin/users' } as RouterStateSnapshot)
    );

    (result as any).subscribe((value: unknown) => {
      expect(router.serializeUrl(value as ReturnType<Router['createUrlTree']>)).toBe('/login?returnUrl=%2Fadmin%2Fusers');
      done();
    });
  });
});

function routeWithData(data: Record<string, unknown>): ActivatedRouteSnapshot {
  return { data } as ActivatedRouteSnapshot;
}
