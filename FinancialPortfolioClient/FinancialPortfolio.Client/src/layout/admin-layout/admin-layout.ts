import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { Footer } from '../components/footer/footer';
import { Navbar } from '../components/navbar/navbar';
import { Sidebar } from '../components/sidebar/sidebar';
import { LayoutService } from '../../core/services/layout/layout.service';
import { SessionExpired } from '../components/session-expired/session-expired';
import { AuthService } from '../../core/services/auth/auth.service';
import { RealtimeService } from '../../core/services/realtime/realtime.service';
import { NotificationService } from '../../core/services/notification/notification.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, Navbar, Sidebar, Footer, SessionExpired],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout implements OnInit, OnDestroy {
  readonly layoutService = inject(LayoutService);
  private readonly auth = inject(AuthService);
  private readonly realtime = inject(RealtimeService);
  // Ensure NotificationService is constructed so it subscribes to SignalR
  private readonly notifications = inject(NotificationService);

  ngOnInit(): void {
    // Admin only — hub is [Authorize(Roles = "Admin")]
    if (this.auth.hasRole('Admin')) {
      void this.realtime.start();
    }
  }

  ngOnDestroy(): void {
    // Keep connection while app lives; stop on logout via AuthService.clearSession
  }
}