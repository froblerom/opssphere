import { HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { TokenStorageService } from '../auth/token-storage.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let tokenStorage: TokenStorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    tokenStorage = TestBed.inject(TokenStorageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('passes API requests through unchanged when no token exists', (done) => {
    const request = new HttpRequest('GET', '/api/health');
    const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 204 })));

    TestBed.runInInjectionContext(() => authInterceptor(request, next)).subscribe(() => {
      expect(next).toHaveBeenCalledOnceWith(request);
      done();
    });
  });

  it('attaches bearer token to API requests', (done) => {
    tokenStorage.setToken(createToken());
    const request = new HttpRequest('GET', '/api/auth/me');
    const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

    TestBed.runInInjectionContext(() => authInterceptor(request, next)).subscribe(() => {
      const forwarded = next.calls.mostRecent().args[0] as HttpRequest<unknown>;
      expect(forwarded.headers.get('Authorization')).toContain('Bearer ');
      done();
    });
  });

  it('does not attach bearer token to external requests', (done) => {
    tokenStorage.setToken(createToken());
    const request = new HttpRequest('GET', 'https://example.test/resource');
    const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

    TestBed.runInInjectionContext(() => authInterceptor(request, next)).subscribe(() => {
      expect(next).toHaveBeenCalledOnceWith(request);
      done();
    });
  });
});

function createToken(expOffsetSeconds = 3600): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  const encodedPayload = btoa(JSON.stringify(payload)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

  return `header.${encodedPayload}.signature`;
}
