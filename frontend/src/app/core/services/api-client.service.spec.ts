import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { ApiClientService } from './api-client.service';

describe('ApiClientService', () => {
  let service: ApiClientService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(ApiClientService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('is created with the configured API base URL', () => {
    expect(service).toBeTruthy();
    expect(service.apiBaseUrl).toBe(environment.apiBaseUrl);
  });

  it('prefixes business API requests with the configured API base URL', () => {
    service.get('foundation-placeholder').subscribe();

    const request = httpTesting.expectOne(`${environment.apiBaseUrl}/foundation-placeholder`);
    expect(request.request.method).toBe('GET');
    request.flush({});
  });
});
