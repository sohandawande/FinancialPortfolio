import {
  AbstractControl,
  AsyncValidatorFn,
  ValidationErrors,
  ValidatorFn,
  FormGroup,
} from '@angular/forms';

export function isFutureIsoDate(value: string): boolean {
  const iso = (value ?? '').trim().substring(0, 10);
  if (!/^\d{4}-\d{2}-\d{2}$/.test(iso)) return false;
  const today = new Date();
  const todayIso = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
  return iso > todayIso;
}
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { AuthService } from '../../services/auth/auth.service';

export class AppValidators {
  static email(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString().trim();
      if (!value) return null;

      const ok = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(value);
      return ok ? null : { email: true };
    };
  }

  static mobile(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString().trim();
      if (!value) return null;
      return /^[0-9]{10}$/.test(value) ? null : { mobile: true };
    };
  }

  /** min 8 + letter + number + symbol */
  static strongPassword(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString();
      if (!value) return null;

      const errors: ValidationErrors = {};
      if (value.length < 8) errors['minLength'] = { requiredLength: 8 };
      if (!/[A-Za-z]/.test(value)) errors['letter'] = true;
      if (!/[0-9]/.test(value)) errors['number'] = true;
      if (!/[^A-Za-z0-9]/.test(value)) errors['symbol'] = true;

      return Object.keys(errors).length ? { strongPassword: errors } : null;
    };
  }

  static match(passwordControlName: string, confirmControlName: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const group = control as FormGroup;
      const password = group.get(passwordControlName);
      const confirm = group.get(confirmControlName);
      if (!password || !confirm) return null;

      if (!confirm.value) return null;

      if (password.value !== confirm.value) {
        confirm.setErrors({ ...(confirm.errors || {}), mismatch: true });
        return { mismatch: true };
      }

      if (confirm.errors) {
        const { mismatch, ...rest } = confirm.errors;
        confirm.setErrors(Object.keys(rest).length ? rest : null);
      }

      return null;
    };
  }

  static userCode(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString().trim();
      if (!value) return null;
      return /^[A-Za-z0-9_-]{3,20}$/.test(value) ? null : { userCode: true };
    };
  }

  /** Username for Identity (server RegisterRequest.UserName) */
  static userName(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString().trim();
      if (!value) return null;
      return /^[A-Za-z0-9._-]{3,30}$/.test(value) ? null : { userName: true };
    };
  }

  static minValue(min: number, inclusive = false): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const raw = control.value;
      if (raw === null || raw === undefined || raw === '') return null;
      const n = Number(raw);
      if (!Number.isFinite(n)) return { minValue: { min } };
      if (inclusive ? n < min : n <= min) return { minValue: { min } };
      return null;
    };
  }

  static dateNotFuture(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString().trim();
      if (!value) return null;
      return isFutureIsoDate(value) ? { futureDate: true } : null;
    };
  }

  static personName(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = (control.value ?? '').toString().trim();
      if (!value) return null;
      return /^[A-Za-z ]{2,100}$/.test(value) ? null : { personName: true };
    };
  }

  static emailAvailable(authService: AuthService): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      const email = (control.value ?? '').toString().trim();
      if (!email) return of(null);

      if (!/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(email)) {
        return of(null);
      }

      return timer(400).pipe(
        switchMap(() => authService.checkEmailAvailable(email)),
        map((available) => (available ? null : { emailTaken: true })),
        catchError(() => of(null)),
      );
    };
  }

  static userNameAvailable(authService: AuthService): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      const name = (control.value ?? '').toString().trim();
      if (!name) return of(null);

      if (!/^[A-Za-z0-9._-]{3,30}$/.test(name)) {
        return of(null);
      }

      return timer(400).pipe(
        switchMap(() => authService.checkUserNameAvailable(name)),
        map((available) => (available ? null : { userNameTaken: true })),
        catchError(() => of(null)),
      );
    };
  }
}