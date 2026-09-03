import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';

import { PageHeader } from '../../layout/components/page-header/page-header';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { WealthService } from '../../core/services/wealth/wealth.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { WealthSummary } from '../../core/models/wealth/wealth.models';

const LOG_FILE = 'wealth.ts';

@Component({
  selector: 'app-wealth',
  standalone: true,
  imports: [CommonModule, PageHeader, CurrencyPipe, DecimalPipe],
  templateUrl: './wealth.html',
  styleUrl: './wealth.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Wealth implements OnInit {
  private readonly wealth = inject(WealthService);
  private readonly router = inject(Router);
  private readonly logger = inject(ClientLoggerService);

  readonly loading = signal(true);
  readonly summary = signal<WealthSummary | null>(null);

  readonly headerActions: PageHeaderAction[] = [
    { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary' },
    { id: 'stocks', label: 'Stocks', icon: 'bi-graph-up-arrow', color: 'outline-secondary' },
    { id: 'etfs', label: 'ETFs', icon: 'bi-bar-chart-steps', color: 'outline-secondary' },
    { id: 'portfolio', label: 'Portfolio', icon: 'bi-briefcase', color: 'outline-secondary' },
    { id: 'mf', label: 'Mutual funds', icon: 'bi-pie-chart', color: 'outline-secondary' },
    { id: 'fd', label: 'FD', icon: 'bi-bank', color: 'outline-secondary' },
    { id: 'rd', label: 'RD', icon: 'bi-calendar-check', color: 'outline-secondary' },
  { id: 'insurance', label: 'Insurance', icon: 'bi-shield-check', color: 'outline-secondary' },
  ];

  ngOnInit(): void {
    this.load();
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'stocks') void this.router.navigate(['/stocks']);
    if (id === 'etfs') void this.router.navigate(['/etfs']);
    if (id === 'portfolio') void this.router.navigate(['/portfolio']);
    if (id === 'mf') void this.router.navigate(['/mutual-funds']);
    if (id === 'fd') void this.router.navigate(['/fixed-deposits']);
    if (id === 'rd') void this.router.navigate(['/recurring-deposits']);
    if (id === 'insurance') void this.router.navigate(['/insurance-policies']);
  }

  openBucket(key: string): void {
    if (key === 'equity' || key === 'stocks') void this.router.navigate(['/holdings']);
    if (key === 'etf' || key === 'etfs') void this.router.navigate(['/etfs']);
    if (key === 'mf') void this.router.navigate(['/mutual-funds']);
    if (key === 'fd') void this.router.navigate(['/fixed-deposits']);
    if (key === 'rd') void this.router.navigate(['/recurring-deposits']);
    if (key === 'insurance') void this.router.navigate(['/insurance-policies']);
  }

  load(): void {
    this.loading.set(true);
    this.wealth.summary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.logger.error('Wealth summary failed', err, LOG_FILE, 'load');
        this.loading.set(false);
      },
    });
  }
}
