import { AbstractControl } from '@angular/forms';

/**
 * Client-side control error → user-facing message.
 */
export function getControlError(
  control: AbstractControl | null,
  fieldLabel = 'Field',
): string {
  if (!control || !control.errors || !(control.dirty || control.touched)) {
    return '';
  }

  const errors = control.errors;

  if (errors['required']) return `${fieldLabel} is required`;
  if (errors['email']) return 'Enter a valid email address';
  if (errors['mobile']) return 'Mobile must be 10 digits';
  if (errors['userCode']) return 'User code: 3–20 letters, numbers, _ or -';
  if (errors['userName']) return 'Username: 3–30 letters, numbers, . _ or -';
  if (errors['personName']) return `${fieldLabel} must be letters only (min 2)`;
  if (errors['mismatch']) return 'Passwords do not match';
  if (errors['minlength']) {
    return `${fieldLabel} must be at least ${errors['minlength'].requiredLength} characters`;
  }
  if (errors['maxlength']) {
    return `${fieldLabel} must be at most ${errors['maxlength'].requiredLength} characters`;
  }
  if (errors['sameAsCurrent']) {
    return 'New password must be different from current password';
  }
  if (errors['strongPassword']) {
    const sp = errors['strongPassword'];
    if (sp.minLength) return 'Password must be at least 8 characters';
    if (sp.letter) return 'Password must include a letter';
    if (sp.number) return 'Password must include a number';
    if (sp.symbol) return 'Password must include a symbol (!@#$…)';
    return 'Password is not strong enough';
  }
  if (errors['emailTaken']) return 'Email is already registered';
  if (errors['userCodeTaken']) return 'User code is already taken';
  if (errors['userNameTaken']) return 'Username is already taken';
  if (errors['min']) return `${fieldLabel} must be at least ${errors['min'].min}`;
  if (errors['max']) return `${fieldLabel} cannot be more than ${errors['max'].max}`;
  if (errors['minValue']) return `${fieldLabel} must be greater than 0`;
  if (errors['futureDate']) return `${fieldLabel} cannot be in the future`;
  if (errors['pastDate']) return `${fieldLabel} cannot be in the past`;
  if (errors['stock']) return 'Select a stock';
  if (errors['etf']) return 'Select an ETF';
  if (errors['pattern']) return `${fieldLabel} format is invalid`;
  if (errors['server']) return String(errors['server']);

  return `${fieldLabel} is invalid`;
}

/**
 * Simple field error for template-driven / signal forms (touched map).
 */
export function fieldError(
  touched: Record<string, boolean> | Set<string> | null | undefined,
  field: string,
  value: unknown,
  rules: {
    required?: boolean;
    min?: number;
    max?: number;
    minLength?: number;
    maxLength?: number;
    email?: boolean;
    pattern?: RegExp;
    custom?: () => string | null;
  } = {},
  label = 'Field',
): string {
  const isTouched =
    touched instanceof Set
      ? touched.has(field)
      : !!(touched && (touched as Record<string, boolean>)[field]);

  if (!isTouched) return '';

  if (rules.custom) {
    const msg = rules.custom();
    if (msg) return msg;
  }

  const str = value == null ? '' : String(value).trim();
  const num = typeof value === 'number' ? value : Number(value);

  if (rules.required && (value === null || value === undefined || str === '')) {
    return `${label} is required`;
  }
  if (rules.email && str && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str)) {
    return 'Enter a valid email address';
  }
  if (rules.minLength != null && str.length < rules.minLength) {
    return `${label} must be at least ${rules.minLength} characters`;
  }
  if (rules.maxLength != null && str.length > rules.maxLength) {
    return `${label} must be at most ${rules.maxLength} characters`;
  }
  if (rules.min != null && !Number.isNaN(num) && num < rules.min) {
    return `${label} must be at least ${rules.min}`;
  }
  if (rules.max != null && !Number.isNaN(num) && num > rules.max) {
    return `${label} cannot be more than ${rules.max}`;
  }
  if (rules.pattern && str && !rules.pattern.test(str)) {
    return `${label} format is invalid`;
  }
  return '';
}
