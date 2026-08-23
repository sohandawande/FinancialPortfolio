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

import { SystemLog } from '../../../core/models/system-log/system-log.model';
import { SystemLogService } from '../../../core/services/logs/system-log.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { PageHeader } from '../../../layout/components/page-header/page-header';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';

@Component({
  selector: 'app-system-log-detail',
  standalone: true,
  imports: [CommonModule, PageHeader],
  templateUrl: './system-log-details.html',
  styleUrl: './system-log-details.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemLogDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly systemLogService = inject(SystemLogService);
  private readonly logger = inject(ClientLoggerService);
  private readonly toast = inject(ToastService);

  readonly log = signal<SystemLog | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  readonly levelBadgeClass = computed(() => {
    const level = (this.log()?.logLevel ?? '').toLowerCase();
    if (level.includes('error') || level.includes('critical')) return 'text-bg-danger';
    if (level.includes('warn')) return 'text-bg-warning';
    if (level.includes('info')) return 'text-bg-info';
    if (level.includes('debug') || level.includes('trace')) return 'text-bg-secondary';
    return 'text-bg-primary';
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => [
  {
    id: 'back',
    label: 'Back to System Logs',
    icon: 'bi-arrow-left',
    color: 'outline-secondary',
  },
]);
  
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

    this.systemLogService.getById(id).subscribe({
      next: (data) => {
        if (!data) {
          this.notFound.set(true);
          this.log.set(null);
        } else {
          this.log.set(data);
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.notFound.set(true);
        this.log.set(null);
        this.toast.error('Failed to load log detail');
        this.logger.error('Load system log detail failed', err, 'SystemLogs');
      },
    });
  }

  back(): void {
    void this.router.navigate(['/system-logs']);
  }

  copy(text: string | null | undefined): void {
    if (!text) return;
    void navigator.clipboard.writeText(text).then(() => {
      this.toast.success('Copied to clipboard');
    });
  }

  onHeaderAction(id: string): void {
  if (id === 'back') this.back();
}
}
