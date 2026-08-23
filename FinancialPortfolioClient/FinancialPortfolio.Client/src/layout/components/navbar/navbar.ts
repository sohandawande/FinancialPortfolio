import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LayoutService } from '../../../core/services/layout/layout.service';
import { Notification } from '../notification/notification';
import { UserMenu } from '../user-menu/user-menu';
import { ThemeToggle } from '../theme-toggle/theme-toggle';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, Notification, UserMenu, ThemeToggle],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Navbar {
  readonly layoutService = inject(LayoutService);

  onToggleSidebar(): void {
    this.layoutService.toggleSidebar();
  }
}