import {
  Component,
  input,
  output,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './page-header.html',
  styleUrl: './page-header.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly actions = input<PageHeaderAction[]>([]);

  readonly actionClick = output<string>(); // emits action.id

  readonly visibleActions = computed(() =>
    (this.actions() ?? []).filter((a) => a.visible !== false)
  );

  onClick(action: PageHeaderAction): void {
    if (action.disabled || action.loading) return;
    this.actionClick.emit(action.id);
  }

  btnClass(action: PageHeaderAction): string {
    const color = action.color || 'outline-secondary';
    return `btn btn-sm btn-${color}`;
  }
}