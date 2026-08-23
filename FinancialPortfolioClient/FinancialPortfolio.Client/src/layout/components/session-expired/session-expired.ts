import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessionExpiryService } from '../../../core/services/auth/session-expiry.service';

@Component({
  selector: 'app-session-expired',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './session-expired.html',
  styleUrl: './session-expired.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SessionExpired {
  readonly session = inject(SessionExpiryService);

  logoutNow(): void {
    this.session.logoutNow();
  }
}