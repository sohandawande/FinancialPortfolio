import {
  Component,
  inject,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';

import { AuthService } from '../../core/services/auth/auth.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { AppValidators } from '../../core/helper/validators/app.validators';
import { getControlError } from '../../core/helper/validators/validation-messages.helper';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import { PasswordStrength } from '../../layout/components/password-strength/password-strength';
import { ChangePasswordRequest } from '../../core/models/auth/change-password-request';

const LOG_FILE = 'change-password.ts';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, PasswordStrength],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePassword {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly logger = inject(ClientLoggerService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly serverError = signal('');
  readonly showCurrent = signal(false);
  readonly showNew = signal(false);
  readonly showConfirm = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, AppValidators.strongPassword()]],
      confirmPassword: ['', [Validators.required]],
    },
    {
      validators: [
        AppValidators.match('newPassword', 'confirmPassword'),
        ChangePassword.differentFromCurrentValidator(),
      ],
    }
  );

  /** New password must not equal current password */
  private static differentFromCurrentValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const current = group.get('currentPassword');
      const next = group.get('newPassword');
      if (!current || !next) {
        return null;
      }

      const nextErrors = { ...(next.errors ?? {}) };

      if (current.value && next.value && current.value === next.value) {
        next.setErrors({ ...nextErrors, sameAsCurrent: true });
        return { sameAsCurrent: true };
      }

      if (nextErrors['sameAsCurrent']) {
        delete nextErrors['sameAsCurrent'];
        next.setErrors(Object.keys(nextErrors).length ? nextErrors : null);
      }

      return null;
    };
  }

  error(name: string, label: string): string {
    return getControlError(this.form.get(name), label);
  }

  get newPasswordValue(): string {
    return this.form.controls.newPassword.value;
  }

  submit(): void {
    this.serverError.set('');
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.toast.warning('Please fix the highlighted fields');
      return;
    }

    this.loading.set(true);

    const v = this.form.getRawValue();
    const request: ChangePasswordRequest = {
      currentPassword: v.currentPassword,
      newPassword: v.newPassword,
      confirmPassword: v.confirmPassword,
    };

    this.auth.changePassword(request).subscribe({
      next: (res) => {
        this.loading.set(false);

        if (res.success) {
          this.toast.success(res.message || 'Password changed successfully');
          this.logger.info('Password changed', LOG_FILE, 'submit');
          this.form.reset();
          void this.router.navigate(['/dashboard']);
          return;
        }

        this.serverError.set(res.message);
        this.toast.error(res.message);
        this.logger.warning(res.message, LOG_FILE, 'submit');
      },
      error: (err) => {
        this.loading.set(false);
        this.serverError.set(apiErrorMessage(err, 'Failed to change password'));
        this.logger.error('Change password failed', err, LOG_FILE, 'submit');
      },
    });
  }
}