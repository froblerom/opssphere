import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorEnvelope, GenericSafeCode, GenericSafeMessage, SafeApiError } from '../models/api-error.models';

// SECURITY NOTE: Never expose raw exception messages, stack traces, or technical details to users.
// Always return safe, user-facing messages.

@Injectable({
  providedIn: 'root'
})
export class ApiErrorParserService {
  parse(errorResponse: HttpErrorResponse | unknown): SafeApiError {
    if (errorResponse instanceof HttpErrorResponse) {
      return this.parseHttpError(errorResponse);
    }

    return this.genericError();
  }

  parseBody(body: unknown): SafeApiError {
    if (body && typeof body === 'object' && 'error' in body) {
      const envelope = body as ApiErrorEnvelope;
      const err = envelope.error;

      if (err && typeof err.code === 'string' && typeof err.message === 'string') {
        return {
          code: err.code,
          message: err.message,
          details: Array.isArray(err.details) ? err.details : [],
          correlationId: err.correlationId
        };
      }
    }

    return this.genericError();
  }

  private parseHttpError(response: HttpErrorResponse): SafeApiError {
    // Attempt to parse the canonical error envelope from the response body
    if (response.error && typeof response.error === 'object') {
      const parsed = this.parseBody(response.error);
      if (parsed.code !== GenericSafeCode || this.isCanonicalEnvelope(response.error)) {
        return parsed;
      }
    }

    // Status-based fallback messages — safe, no raw exception text
    if (response.status === 401) {
      return { code: 'unauthorized', message: 'You are not authenticated. Please log in.', details: [] };
    }

    if (response.status === 403) {
      return { code: 'forbidden', message: 'You do not have permission to perform this action.', details: [] };
    }

    if (response.status === 404) {
      return { code: 'not_found', message: 'The requested resource was not found.', details: [] };
    }

    if (response.status === 0) {
      return { code: 'network_error', message: 'Unable to reach the server. Please check your connection.', details: [] };
    }

    return this.genericError();
  }

  private isCanonicalEnvelope(body: unknown): boolean {
    return (
      !!body &&
      typeof body === 'object' &&
      'error' in body &&
      !!(body as ApiErrorEnvelope).error?.code
    );
  }

  private genericError(): SafeApiError {
    return {
      code: GenericSafeCode,
      message: GenericSafeMessage,
      details: []
    };
  }
}
