import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

import { WealthService } from '../../core/services/wealth/wealth.service';
import { WealthSummary } from '../../core/models/wealth/wealth.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe, DecimalPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard implements OnInit {
  private readonly wealth = inject(WealthService);

  readonly loading = signal(true);
  readonly summary = signal<WealthSummary | null>(null);

  readonly greeting = (() => {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 17) return 'Good afternoon';
    return 'Good evening';
  })();

  readonly todayLabel = new Date().toLocaleDateString('en-IN', {
    weekday: 'long',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.wealth.summary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.summary.set(null);
        this.loading.set(false);
      },
    });
  }

  bucket(key: string): { currentValue: number; invested: number; count: number; allocationPercent: number } | null {
    const b = this.summary()?.buckets?.find((x) => x.key === key);
    return b ?? null;
  }
}
