import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorParserService } from './api-error-parser.service';
import { GenericSafeCode, GenericSafeMessage } from '../models/api-error.models';

describe('ApiErrorParserService', () => {
  let service: ApiErrorParserService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ApiErrorParserService);
  });

  // Parse validation error envelope
  it('parses validation error envelope with details', () => {
    const body = {
      error: {
        code: 'validation_error',
        message: 'The request contains validation errors.',
        details: [{ field: 'email', message: 'Email is required.' }],
        correlationId: 'abc-123'
      }
    };

    const result = service.parseBody(body);

    expect(result.code).toBe('validation_error');
    expect(result.message).toBe('The request contains validation errors.');
    expect(result.details).toHaveSize(1);
    expect(result.details[0].field).toBe('email');
    expect(result.correlationId).toBe('abc-123');
  });

  // Parse simple error envelope (no details)
  it('parses error envelope without details', () => {
    const body = {
      error: {
        code: 'not_found',
        message: 'The requested resource was not found.',
        correlationId: 'def-456'
      }
    };

    const result = service.parseBody(body);

    expect(result.code).toBe('not_found');
    expect(result.details).toEqual([]);
    expect(result.correlationId).toBe('def-456');
  });

  // Preserve correlationId from envelope
  it('preserves correlationId from envelope', () => {
    const body = {
      error: {
        code: 'conflict',
        message: 'Duplicate email.',
        correlationId: 'corr-xyz-789'
      }
    };

    const result = service.parseBody(body);

    expect(result.correlationId).toBe('corr-xyz-789');
  });

  // Fall back to generic safe message for unknown/raw errors
  it('falls back to generic safe message for unknown error object', () => {
    const result = service.parseBody({ something: 'unexpected' });

    expect(result.code).toBe(GenericSafeCode);
    expect(result.message).toBe(GenericSafeMessage);
    expect(result.details).toEqual([]);
  });

  // Fall back for null/undefined
  it('falls back to generic safe message for null body', () => {
    const result = service.parseBody(null);

    expect(result.code).toBe(GenericSafeCode);
    expect(result.message).toBe(GenericSafeMessage);
  });

  // Parse HttpErrorResponse with canonical envelope
  it('parses HttpErrorResponse containing a canonical error envelope', () => {
    const errorResponse = new HttpErrorResponse({
      error: {
        error: {
          code: 'business_rule_violation',
          message: 'Ticket must be resolved before closing.',
          correlationId: 'biz-001'
        }
      },
      status: 400,
      statusText: 'Bad Request'
    });

    const result = service.parse(errorResponse);

    expect(result.code).toBe('business_rule_violation');
    expect(result.correlationId).toBe('biz-001');
  });

  // Returns safe 401 message for unauthenticated error without envelope
  it('returns safe unauthorized message for 401 without envelope', () => {
    const errorResponse = new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' });

    const result = service.parse(errorResponse);

    expect(result.code).toBe('unauthorized');
    expect(result.message).not.toContain('stack');
    expect(result.message).not.toContain('Exception');
  });

  // Returns safe 403 message
  it('returns safe forbidden message for 403', () => {
    const errorResponse = new HttpErrorResponse({ status: 403, statusText: 'Forbidden' });

    const result = service.parse(errorResponse);

    expect(result.code).toBe('forbidden');
  });

  // Returns safe network error message for status 0
  it('returns safe network error message for status 0', () => {
    const errorResponse = new HttpErrorResponse({ status: 0, statusText: 'Unknown Error' });

    const result = service.parse(errorResponse);

    expect(result.code).toBe('network_error');
  });

  // Does not expose raw stack traces from unknown error objects
  it('does not expose raw stack trace from unknown error object', () => {
    const rawException = new Error('Internal implementation detail');

    const result = service.parse(rawException);

    expect(result.message).not.toContain('Internal implementation detail');
    expect(result.message).toBe(GenericSafeMessage);
    expect(result.code).toBe(GenericSafeCode);
  });
});
