import { Injectable, computed, signal } from '@angular/core';

import { LAYOUT } from '../../constants/layout/layout.constants';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  readonly screenWidth = signal(window.innerWidth);

  readonly sidebarCollapsed = signal(false);

  readonly drawerOpen = signal(false);

  readonly isDesktop = computed(() => this.screenWidth() >= 992);

  readonly isTablet = computed(() => this.screenWidth() >= 768 && this.screenWidth() < 992);

  readonly isMobile = computed(() => this.screenWidth() < 768);

  readonly sidebarWidth = computed(() => {
    if (!this.isDesktop()) {
      return LAYOUT.SIDEBAR_WIDTH;
    }

    return this.sidebarCollapsed() ? LAYOUT.SIDEBAR_COLLAPSED_WIDTH : LAYOUT.SIDEBAR_WIDTH;
  });

  updateScreen(width: number): void {
    this.screenWidth.set(width);

    if (this.isDesktop()) {
      this.drawerOpen.set(false);
    }
  }

  toggleSidebar(): void {
    if (this.isDesktop()) {
      this.sidebarCollapsed.update((v) => !v);

      return;
    }

    this.drawerOpen.update((v) => !v);
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
  }
}
