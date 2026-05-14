import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiClientService {
  private readonly http = inject(HttpClient);

  readonly apiBaseUrl = environment.apiBaseUrl;

  get<TResponse>(path: string) {
    return this.http.get<TResponse>(this.buildUrl(path));
  }

  post<TRequest, TResponse>(path: string, body: TRequest) {
    return this.http.post<TResponse>(this.buildUrl(path), body);
  }

  put<TRequest, TResponse>(path: string, body: TRequest) {
    return this.http.put<TResponse>(this.buildUrl(path), body);
  }

  delete<TResponse>(path: string) {
    return this.http.delete<TResponse>(this.buildUrl(path));
  }

  private buildUrl(path: string) {
    const normalizedPath = path.startsWith('/') ? path : `/${path}`;

    return `${this.apiBaseUrl}${normalizedPath}`;
  }
}
