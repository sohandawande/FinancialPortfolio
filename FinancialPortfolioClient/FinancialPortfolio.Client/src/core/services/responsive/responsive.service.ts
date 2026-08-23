import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ResponsiveService {
  readonly isMobile = signal(false);

  readonly isTablet = signal(false);

  readonly isDesktop = signal(true);

  update(width: number): void {
    this.isMobile.set(width < 768);

    this.isTablet.set(width >= 768 && width < 992);

    this.isDesktop.set(width >= 992);
  }
}
