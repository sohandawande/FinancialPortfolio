import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingOverlayService {
  readonly active = signal(false);
  private count = 0;

  show(): void {
    this.count++;
    this.active.set(true);
  }

  hide(): void {
    this.count = Math.max(0, this.count - 1);
    if (this.count === 0) {
      this.active.set(false);
    }
  }

  reset(): void {
    this.count = 0;
    this.active.set(false);
  }
}