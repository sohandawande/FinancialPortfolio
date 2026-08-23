import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { Stock } from '../../../core/models/stock/stock.model';
import { StockService } from '../../../core/services/stock/stock.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { environment } from '../../../environments/environment';
import { PageHeader } from '../../../layout/components/page-header/page-header';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';

const LOG_FILE = 'stock-detail.ts';

@Component({
  selector: 'app-stock-detail',
  standalone: true,
  imports: [CommonModule, PageHeader],
  templateUrl: './stock-details.html',
  styleUrl: './stock-details.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StockDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly stockService = inject(StockService);
  private readonly toast = inject(ToastService);
  private readonly logger = inject(ClientLoggerService);

  readonly stock = signal<Stock | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  readonly isUp = computed(() => (this.stock()?.priceChange ?? 0) > 0);
  readonly isDown = computed(() => (this.stock()?.priceChange ?? 0) < 0);
  readonly isFlat = computed(() => (this.stock()?.priceChange ?? 0) === 0);

  readonly headerActions = computed<PageHeaderAction[]>(() => [
  {
    id: 'back',
    label: 'Back to stocks',
    icon: 'bi-arrow-left',
    color: 'outline-secondary',
  },
]);

  readonly changePercent = computed(() => {
    const s = this.stock();
    if (!s || !s.previousClose) return 0;
    return (s.priceChange / s.previousClose) * 100;
  });

  readonly dayRangePct = computed(() => {
    const s = this.stock();
    if (!s) return 0;
    const { lowPrice, highPrice, currentPrice } = s;
    if (!highPrice || highPrice <= lowPrice) return 50;
    return Math.min(
      100,
      Math.max(0, ((currentPrice - lowPrice) / (highPrice - lowPrice)) * 100)
    );
  });

  readonly weekRangePct = computed(() => {
    const s = this.stock();
    if (!s) return 0;
    const { week52Low, week52High, currentPrice } = s;
    if (!week52High || week52High <= week52Low) return 50;
    return Math.min(
      100,
      Math.max(
        0,
        ((currentPrice - week52Low) / (week52High - week52Low)) * 100
      )
    );
  });

  readonly logoUrl = computed(() => {
  const s = this.stock();
  console.log('logoUrl computed', s);
  if (s?.logoUrl) {
    if (s.logoUrl.startsWith('http')) {
      return s.logoUrl;
    }
    // strip /api from environment.apiUrl → https://localhost:7xxx
    const base = environment.apiUrl.replace(/\/api\/?$/, '');
    return `${base}${s.logoUrl.startsWith('/') ? '' : '/'}${s.logoUrl}`;
  }
  return this.avatarUrl(s?.symbol || 'ST');
});

private avatarUrl(symbol: string): string {
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(symbol)}&background=1d4ed8&color=fff&bold=true&size=128`;
}

onLogoError(ev: Event): void {
  const img = ev.target as HTMLImageElement;
  img.src = this.avatarUrl(this.stock()?.symbol || 'ST');
}

  readonly initials = computed(() => {
    const s = this.stock()?.symbol || 'ST';
    return s.slice(0, 2).toUpperCase();
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id || Number.isNaN(id)) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }
    this.load(id);
  }

  load(id: number): void {
    this.loading.set(true);
    this.notFound.set(false);

    this.stockService.getById(id).subscribe({
      next: (data) => {
        if (!data) {
          this.notFound.set(true);
          this.stock.set(null);
        } else {
          this.stock.set(data);
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.notFound.set(true);
        this.toast.error('Failed to load stock');
        this.logger.error('Load stock detail failed', err, LOG_FILE, 'load');
      },
    });
  }

  back(): void {
    void this.router.navigate(['/stocks']);
  }

  money(v: number | null | undefined): string {
    if (v == null || isNaN(Number(v))) return '—';
    return Number(v).toLocaleString('en-IN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
  }

  num(v: number | null | undefined): string {
    if (v == null || isNaN(Number(v))) return '—';
    return Number(v).toLocaleString('en-IN');
  }

  onHeaderAction(id: string): void {
  if (id === 'back') this.back();
}
}