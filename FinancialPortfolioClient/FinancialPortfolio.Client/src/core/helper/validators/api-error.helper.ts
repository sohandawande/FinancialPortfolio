import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from '../../models/common/api-response';

/**
 * Extract a clear user-facing message from API / HTTP errors.
 * Prefer: errors[] → message (if not generic) → ProblemDetails → status → fallback.
 */
export function apiErrorMessage(
  source: unknown,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (source == null) return fallback;

  if (typeof source === 'string' && source.trim()) {
    return source.trim();
  }

  const body = isHttpError(source) ? source.error : source;

  if (typeof body === 'string' && body.trim()) {
    return body.length < 400 ? body.trim() : fallback;
  }

  if (body && typeof body === 'object') {
    const response = body as Partial<ApiResponse<unknown>> & {
      title?: string;
      detail?: string;
      Message?: string;
      Errors?: string[] | Record<string, string[]>;
      errors?: string[] | Record<string, string[]>;
    };

    const fromErrors = firstError(response.errors ?? response.Errors);
    if (fromErrors) return fromErrors;

    const message = (response.message || response.Message || response.detail || '').trim();
    if (message && !isGenericValidationMessage(message)) return message;

    if (response.title && response.title.trim() && !isGenericValidationMessage(response.title)) {
      return response.title.trim();
    }

    if (message) return message;
  }

  if (source instanceof HttpErrorResponse || isHttpError(source)) {
    const status = (source as HttpErrorResponse).status;
    if (status === 0) return 'Network error. Check your connection.';
    if (status === 401) return 'Session expired. Please sign in again.';
    if (status === 403) return 'You do not have permission to perform this action.';
    if (status === 404) return 'The requested resource was not found.';
    if (status === 409) return 'Conflict. The record may already exist.';
    if (status === 422) return 'Validation failed. Check the form and try again.';
    if (status >= 500) return 'Server error. Please try again later.';
    const st = (source as HttpErrorResponse).statusText;
    if (st) return st;
  }

  if (source instanceof Error && source.message) return source.message;

  return fallback;
}

/** Prefer API body message when success is false. */
export function messageFromApiResponse(
  res: Partial<ApiResponse<unknown>> | null | undefined,
  fallback = 'Request failed',
): string {
  if (!res) return fallback;
  const fromErrors = firstError(res.errors);
  if (fromErrors) return fromErrors;
  if (res.message?.trim()) return res.message.trim();
  return fallback;
}

function firstError(
  errors: string[] | Record<string, string[]> | undefined | null,
): string {
  if (!errors) return '';
  if (Array.isArray(errors)) {
    const msg = errors.map((e) => String(e).trim()).find(Boolean);
    return msg || '';
  }
  if (typeof errors === 'object') {
    for (const val of Object.values(errors)) {
      if (Array.isArray(val) && val[0]) return String(val[0]);
      if (val) return String(val);
    }
  }
  return '';
}

function isHttpError(value: unknown): value is { error: unknown; status?: number; statusText?: string } {
  return typeof value === 'object' && value !== null && 'error' in value;
}

function isGenericValidationMessage(message: string): boolean {
  const n = message.toLowerCase().trim();
  return (
    n === 'validation failed.' ||
    n === 'validation failed' ||
    n === 'one or more validation failures occurred.' ||
    n === 'one or more validation errors occurred.'
  );
}
