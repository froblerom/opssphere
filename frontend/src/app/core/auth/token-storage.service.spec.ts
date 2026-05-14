import { TestBed } from '@angular/core/testing';

import { TokenStorageService } from './token-storage.service';

describe('TokenStorageService', () => {
  let service: TokenStorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenStorageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('stores and clears the token', () => {
    const token = createToken();

    service.setToken(token);

    expect(service.getToken()).toBe(token);
    expect(service.getUsableToken()).toBe(token);

    service.clearToken();

    expect(service.getToken()).toBeNull();
  });

  it('clears expired tokens when reading usable token', () => {
    service.setToken(createToken(-60));

    expect(service.getUsableToken()).toBeNull();
    expect(service.getToken()).toBeNull();
  });
});

function createToken(expOffsetSeconds = 3600): string {
  const payload = { exp: Math.floor(Date.now() / 1000) + expOffsetSeconds };
  const encodedPayload = btoa(JSON.stringify(payload)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

  return `header.${encodedPayload}.signature`;
}
