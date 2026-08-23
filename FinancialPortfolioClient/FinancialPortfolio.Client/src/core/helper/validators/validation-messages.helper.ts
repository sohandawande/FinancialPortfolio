import { AbstractControl } from '@angular/forms';

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
  if (errors['stock']) return 'Select a stock';

  return `${fieldLabel} is invalid`;
}