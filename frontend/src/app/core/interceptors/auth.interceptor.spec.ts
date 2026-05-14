import { HttpRequest, HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';

import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  it('passes requests through unchanged while auth is not implemented', (done) => {
    const request = new HttpRequest('GET', '/api/health');
    const next = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 204 })));

    authInterceptor(request, next).subscribe(() => {
      expect(next).toHaveBeenCalledOnceWith(request);
      done();
    });
  });
});
