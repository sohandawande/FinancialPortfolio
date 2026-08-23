import { Injectable, inject, signal } from '@angular/core';
import { UserService } from './user.service';
import { RealtimeService } from '../realtime/realtime.service';

@Injectable({ providedIn: 'root' })
export class PendingUsersStateService {
  private readonly userService = inject(UserService);
  private readonly realtime = inject(RealtimeService);

  readonly count = signal(0);

  constructor() {
    this.refresh();
    this.realtime.onPendingUserCreated.subscribe(() => this.refresh());
  }

  refresh(): void {
    this.userService.getPendingUsers().subscribe({
      next: (list) => this.count.set(list?.length ?? 0),
      error: () => this.count.set(0),
    });
  }
}