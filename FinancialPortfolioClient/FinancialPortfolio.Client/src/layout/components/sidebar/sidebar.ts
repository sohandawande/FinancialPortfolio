import {
  Component,
  inject,
  HostBinding,
  ChangeDetectionStrategy,
} from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { LayoutService } from '../../../core/services/layout/layout.service';
import { SIDEBAR_MENU } from '../../../core/constants/sidebar-menu/sidebar-menu.constants';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  readonly layoutService = inject(LayoutService);
  private readonly router = inject(Router);
  readonly menus = SIDEBAR_MENU;

  @HostBinding('class.collapsed')
  get collapsed(): boolean {
    return this.layoutService.isDesktop() && this.layoutService.sidebarCollapsed();
  }

  @HostBinding('class.drawer-open')
  get drawerOpen(): boolean {
    return this.layoutService.drawerOpen();
  }

  navigate(): void {
    if (!this.layoutService.isDesktop()) {
      this.layoutService.closeDrawer();
    }
  }
}