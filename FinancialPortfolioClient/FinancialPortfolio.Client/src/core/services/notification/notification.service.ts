import { Injectable, computed, inject, signal, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { AppNotification } from '../../models/notification/notification.model';
import { RealtimeService } from '../realtime/realtime.service';
import { PendingUser } from '../../models/user/pending-user.model';
import { ToastService } from '../toast/toast.service';

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private readonly realtime = inject(RealtimeService);
  private readonly toast = inject(ToastService);

  private seq = 0;
  private sub?: Subscription;

  readonly items = signal<AppNotification[]>([]);

  readonly unreadCount = computed(
    () => this.items().filter((n) => !n.read).length,
  );

  constructor() {
    // Listen for SignalR "PendingUserCreated"
    this.sub = this.realtime.onPendingUserCreated.subscribe((user) => {
      this.handlePendingUser(user);
    });
  }

  private handlePendingUser(user: PendingUser): void {
    const name = user.fullName || user.email || 'New user';
    this.add(
      'New registration',
      `${name} is waiting for approval`,
      'bi-person-plus',
    );
    this.toast.info(`${name} registered and is pending approval`);
  }

  add(title: string, message: string, icon = 'bi-bell'): void {
    const id = ++this.seq;
    this.items.update((list) => [
      {
        id,
        title,
        message,
        time: new Date().toLocaleTimeString(),
        read: false,
        icon,
      },
      ...list,
    ]);
  }

  info(message: string, title = 'Info'): void {
    this.add(title, message, 'bi-info-circle');
  }

  success(message: string, title = 'Success'): void {
    this.add(title, message, 'bi-check-circle');
  }

  warning(message: string, title = 'Warning'): void {
    this.add(title, message, 'bi-exclamation-triangle');
  }

  error(message: string, title = 'Error'): void {
    this.add(title, message, 'bi-x-circle');
  }

  markRead(id: number): void {
    this.items.update((list) =>
      list.map((n) => (n.id === id ? { ...n, read: true } : n)),
    );
  }

  markAllRead(): void {
    this.items.update((list) => list.map((n) => ({ ...n, read: true })));
  }

  clear(): void {
    this.items.set([]);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}