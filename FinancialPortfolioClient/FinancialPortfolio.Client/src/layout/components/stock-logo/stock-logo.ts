import {
  Component,
  ChangeDetectionStrategy,
  input,
  signal,
  computed,
  effect,
} from '@angular/core';

import {
  initialsAvatarUrl,
  resolveStockLogoUrl,
  stockLogoFileUrl,
} from '../../../core/helper/formatters/stock-format.helper';

@Component({
  selector: 'app-stock-logo',
  standalone: true,
  templateUrl: './stock-logo.html',
  styleUrl: './stock-logo.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StockLogo {
  readonly symbol = input('', { alias: 'symbol' });
  readonly logoUrl = input<string | null | undefined>(null);
  readonly size = input(32);
  readonly rounded = input<'sm' | 'md' | 'lg' | 'circle'>('md');

  /** 0 = stored url, 1 = /logos/SYMBOL.png, 2 = give up (initials only) */
  readonly attempt = signal(0);

  readonly src = computed(() => {
    const attempt = this.attempt();
    const symbol = this.symbol();
    if (attempt === 0) return resolveStockLogoUrl(this.logoUrl(), symbol);
    if (attempt === 1) return stockLogoFileUrl(symbol) ?? initialsAvatarUrl(symbol);
    return null;
  });

  readonly initials = computed(
    () =>
      (this.symbol() || 'ST').replace(/[^A-Za-z0-9]/g, '').slice(0, 2).toUpperCase() ||
      'ST',
  );

  readonly radiusClass = computed(() => `is-${this.rounded()}`);
  readonly fontSize = computed(() => Math.max(10, Math.round(this.size() * 0.36)));

  constructor() {
    effect(() => {
      this.logoUrl();
      this.symbol();
      this.attempt.set(0);
    });
  }

  onError(): void {
    this.attempt.update((n) => Math.min(2, n + 1));
  }
}
