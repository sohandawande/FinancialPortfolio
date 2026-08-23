import {
  Component,
  OnInit,
  inject,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../core/services/auth/auth.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { AppValidators } from '../../core/helper/validators/app.validators';
import { getControlError } from '../../core/helper/validators/validation-messages.helper';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import { PasswordStrength } from '../../layout/components/password-strength/password-strength';
import { ResetPasswordRequest } from '../../core/models/auth/reset-password-request';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, PasswordStrength],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPassword implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly logger = inject(ClientLoggerService);

  readonly loading = signal(false);
  readonly serverError = signal('');
  readonly linkInvalid = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, AppValidators.email()]],
      token: ['', [Validators.required]],
      password: ['', [Validators.required, AppValidators.strongPassword()]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: AppValidators.match('password', 'confirmPassword') }
  );

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!email || !token) {
      this.linkInvalid.set(true);
      return;
    }

    this.form.patchValue({ email, token });
  }

  error(name: string, label: string): string {
    return getControlError(this.form.get(name), label);
  }

  get passwordValue(): string {
    return this.form.controls.password.value;
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
    const request: ResetPasswordRequest = {
      email: v.email.trim(),
      token: v.token,
      password: v.password,
      confirmPassword: v.confirmPassword,
    };

    this.auth.resetPassword(request).subscribe({
      next: (res) => {
        this.loading.set(false);

        if (res.success) {
          this.toast.success(res.message || 'Password reset successfully');
          this.logger.info('Password reset success', 'Auth');
          this.router.navigate(['/login']);
          return;
        }

        this.serverError.set(res.message);
        this.toast.error(res.message);
      },
      error: (err) => {
        this.loading.set(false);
        this.serverError.set(apiErrorMessage(err, 'Reset failed. The link may have expired.'));
        this.logger.error('Reset password failed', err, 'Auth');
      },
    });
  }
}