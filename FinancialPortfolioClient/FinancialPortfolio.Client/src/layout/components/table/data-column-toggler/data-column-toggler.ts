import {
  Component,
  input,
  output,
  signal,
  computed,
  HostListener,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableColumn } from '../../../../core/models/query/table-column.model';

@Component({
  selector: 'app-data-column-toggler',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-column-toggler.html',
  styleUrl: './data-column-toggler.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataColumnToggler {
  readonly columns = input<TableColumn[]>([]);
  readonly columnsChange = output<TableColumn[]>();

  readonly open = signal(false);

  readonly items = computed(() =>
    this.columns().filter((c) => c.canToggle !== false && c.type !== 'actions')
  );

  toggleMenu(): void {
    this.open.update((v) => !v);
  }

  toggleColumn(col: TableColumn): void {
    const updated = this.columns().map((c) =>
      c.key === col.key ? { ...c, hidden: !c.hidden } : c
    );
    this.columnsChange.emit(updated);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.data-column-toggler')) {
      this.open.set(false);
    }
  }
}