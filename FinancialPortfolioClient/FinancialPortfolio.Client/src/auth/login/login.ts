import {
  Component,
  inject,
  signal,
  ChangeDetectionStrategy,
  OnInit,
} from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../core/services/auth/auth.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { getControlError } from '../../core/helper/validators/validation-messages.helper';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import { LoginRequest } from '../../core/models/auth/login-request';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly logger = inject(ClientLoggerService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly rememberMe = signal(true);

  readonly form = this.fb.nonNullable.group({
    loginId: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  ngOnInit(): void {
    const reason = this.route.snapshot.queryParamMap.get('reason');
    if (reason === 'session-expired') {
      this.errorMessage.set('Your session expired. Please sign in again.');
    } else if (reason === 'unauthorized') {
      this.errorMessage.set('Please sign in to continue.');
    }
  }

  error(name: string, label: string): string {
    if (name === 'loginId' && this.form.get(name)?.hasError('required') && (this.form.get(name)?.dirty || this.form.get(name)?.touched)) {
      return 'Enter your email, username, or user code';
    }
    return getControlError(this.form.get(name), label);
  }

  login(): void {
    this.form.markAllAsTouched();
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.toast.warning('Enter your login ID and password');
      return;
    }

    this.loading.set(true);

    const request: LoginRequest = {
      loginId: this.form.controls.loginId.value.trim(),
      password: this.form.controls.password.value,
    };

    this.authService.login(request, this.rememberMe()).subscribe({
      next: (success) => {
        this.loading.set(false);

        if (success) {
          this.toast.success('Welcome back');
          this.logger.info('User logged in successfully', 'Auth');
          const returnUrl =
            this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';
          void this.router.navigateByUrl(returnUrl);
        } else {
          this.errorMessage.set('Invalid credentials or this account is not active.');
          this.logger.warning('Login failed - invalid credentials', 'Auth');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(apiErrorMessage(err, 'Unable to sign in. Please try again.'));
        this.logger.error('Login API error', err, 'Auth');
      },
    });
  }
}
