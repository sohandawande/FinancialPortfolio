import {
  Component,
  inject,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { RouterLink } from '@angular/router';
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

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPassword {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly logger = inject(ClientLoggerService);

  readonly loading = signal(false);
  readonly sent = signal(false);
  readonly serverError = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, AppValidators.email()]],
  });

  error(name: string, label: string): string {
    return getControlError(this.form.get(name), label);
  }

  submit(): void {
    this.serverError.set('');
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.toast.warning('Enter a valid email');
      return;
    }

    this.loading.set(true);

    this.auth
      .forgotPassword({ email: this.form.controls.email.value.trim() })
      .subscribe({
        next: (res) => {
          this.loading.set(false);

          // Always show success-style message (security)
          this.sent.set(true);
          this.toast.success(
            res.message ||
              'If the email exists, a reset link has been sent.'
          );
          this.logger.info('Forgot password requested', 'Auth');
        },
        error: (err) => {
          this.loading.set(false);
          this.serverError.set(apiErrorMessage(err, 'Something went wrong. Please try again.'));
          this.logger.error('Forgot password failed', err, 'Auth');
        },
      });
  }
}