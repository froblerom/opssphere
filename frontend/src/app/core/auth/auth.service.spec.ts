import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
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
  });
});

function createToken(expOffsetSeconds = 3600): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  const encodedPayload = btoa(JSON.stringify(payload)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

  return `header.${encodedPayload}.signature`;
}
