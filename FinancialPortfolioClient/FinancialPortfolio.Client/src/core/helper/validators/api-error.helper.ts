import { ApiResponse } from '../../models/common/api-response';

export function apiErrorMessage(
  source: unknown,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (!source) return fallback;

  const body = isHttpError(source) ? source.error : source;
  if (typeof body === 'string' && body.trim()) return body.trim();

  const response = body as Partial<ApiResponse<unknown>> | null;
  const errors = response?.errors?.filter((x) => !!x?.trim());
  if (errors?.length) return errors[0];

  const message = (response?.message || '').trim();
  if (message && !isGenericValidationMessage(message)) return message;
  if (errors?.length) return errors[0];
  if (message) return message;

  return fallback;
}

function isHttpError(value: unknown): value is { error: unknown } {
  return typeof value === 'object' && value !== null && 'error' in value;
}

function isGenericValidationMessage(message: string): boolean {
  const n = message.toLowerCase();
  return (
    n === 'validation failed.' ||
    n === 'validation failed' ||
    n === 'one or more validation failures occurred.'
  );
}
