import {
  Component,
  inject,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../core/services/auth/auth.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { AppValidators } from '../../core/helper/validators/app.validators';
import { getControlError } from '../../core/helper/validators/validation-messages.helper';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import { PasswordStrength } from '../../layout/components/password-strength/password-strength';
import { RegisterRequest } from '../../core/models/auth/register-request';

/**
 * Registration form aligned with server RegisterRequest:
 * userName, email, password, confirmPassword, firstName, lastName, mobileNumber
 * UserCode is generated on the server after successful registration.
 */
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, PasswordStrength],
  templateUrl: './register.html',
  styleUrl: './register.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly logger = inject(ClientLoggerService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);
  readonly serverError = signal('');

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, AppValidators.personName()]],
      lastName: ['', [Validators.required, AppValidators.personName()]],
      email: [
        '',
        {
          validators: [Validators.required, AppValidators.email()],
          asyncValidators: [AppValidators.emailAvailable(this.authService)],
          updateOn: 'blur',
        },
      ],
      userName: [
        '',
        {
          validators: [Validators.required, AppValidators.userName()],
          asyncValidators: [AppValidators.userNameAvailable(this.authService)],
          updateOn: 'blur',
        },
      ],
      mobileNumber: ['', [Validators.required, AppValidators.mobile()]],
      password: ['', [Validators.required, AppValidators.strongPassword()]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: AppValidators.match('password', 'confirmPassword') },
  );

  error(controlName: string, label: string): string {
    return getControlError(this.form.get(controlName), label);
  }

  get passwordValue(): string {
    return this.form.controls.password.value;
  }

  submit(): void {
    this.serverError.set('');
    this.form.markAllAsTouched();

    if (this.form.pending) {
      this.toast.info('Please wait, checking email / username…');
      return;
    }

    if (this.form.invalid) {
      this.toast.warning('Please fix the highlighted fields');
      return;
    }

    this.loading.set(true);

    const value = this.form.getRawValue();
    const request: RegisterRequest = {
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      email: value.email.trim(),
      userName: value.userName.trim(),
      mobileNumber: value.mobileNumber.trim(),
      password: value.password,
      confirmPassword: value.confirmPassword,
    };

    this.authService.register(request).subscribe({
      next: (res) => {
        this.loading.set(false);

        if (res.success) {
          this.toast.success(
            res.message || 'Registered successfully. Please wait for admin approval.',
          );
          this.logger.info('User registered successfully', 'Auth');
          this.router.navigate(['/login']);
          return;
        }

        this.mapServerError(res.message);
      },
      error: (err) => {
        this.loading.set(false);
        this.mapHttpError(err);
        this.logger.error('Register API error', err, 'Auth');
      },
    });
  }

  private mapServerError(message: string): void {
    const msg = (message || '').toLowerCase();

    if (msg.includes('email')) {
      this.form.controls.email.setErrors({ emailTaken: true });
      this.form.controls.email.markAsTouched();
      this.toast.error('Email is already registered');
      return;
    }

    if (msg.includes('user name') || msg.includes('username')) {
      this.form.controls.userName.setErrors({ userNameTaken: true });
      this.form.controls.userName.markAsTouched();
      this.toast.error('Username is already taken');
      return;
    }

    this.serverError.set(message || 'Registration failed');
    this.toast.error(message || 'Registration failed');
  }

  private mapHttpError(err: unknown): void {
    this.mapServerError(apiErrorMessage(err, 'Something went wrong. Please try again.'));
  }
}