import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { provideRouter, Router } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { authGuard } from './auth.guard';

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
