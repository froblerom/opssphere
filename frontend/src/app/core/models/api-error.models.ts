// Canonical API error envelope — matches backend OpsSphere.Api.Common.ApiErrorEnvelope.
// SECURITY NOTE: Parse and display safe messages only. Never expose raw stack traces or exception details to users.

export interface ApiErrorEnvelope {
  error: {
    code: string;
    message: string;
    details?: ApiErrorDetail[];
    correlationId?: string;
  };
}

export interface ApiErrorDetail {
  field?: string;
  message: string;
}

export interface SafeApiError {
  code: string;
  message: string;
  details: ApiErrorDetail[];
  correlationId?: string;
}

export const GenericSafeMessage = 'An unexpected error occurred. Please try again or contact support.';
export const GenericSafeCode = 'unexpected_error';
