import {
  Component,
  input,
  output,
  computed,
  signal,
  effect,
  ChangeDetectionStrategy,
  ElementRef,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-data-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './data-pagination.html',
  styleUrl: './data-pagination.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class DataPagination {
  private readonly host = inject(ElementRef<HTMLElement>);
  readonly pageNumber = input<number>(1);
  readonly pageSize = input<number>(10);
  readonly totalRecords = input<number>(0);
  readonly pageSizeOptions = input<number[]>([5, 10, 25, 50, 100]);

  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();

  /** Jump-to input (synced with current page) */
  readonly jumpPage = signal(1);
  readonly sizeOpen = signal(false);

  constructor() {
    effect(() => {
      this.jumpPage.set(this.pageNumber());
    });
  }

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil((this.totalRecords() || 0) / (this.pageSize() || 10)))
  );

  /** "Showing 1 to 5 of 500" */
  readonly fromRecord = computed(() => {
    if (this.totalRecords() === 0) return 0;
    return (this.pageNumber() - 1) * this.pageSize() + 1;
  });

  readonly toRecord = computed(() =>
    Math.min(this.pageNumber() * this.pageSize(), this.totalRecords())
  );

  readonly rangeText = computed(() => {
    if (this.totalRecords() === 0) return '0 of 0';
    return `${this.fromRecord()}–${this.toRecord()} of ${this.totalRecords()}`;
  });

  /** Window of page buttons around current */
  readonly pages = computed(() => {
    const result: number[] = [];
    const current = this.pageNumber();
    const total = this.totalPages();
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) result.push(i);
    return result;
  });

  readonly isFirst = computed(() => this.pageNumber() <= 1);
  readonly isLast = computed(() => this.pageNumber() >= this.totalPages());

  goTo(page: number): void {
    const p = Math.floor(Number(page));
    if (p >= 1 && p <= this.totalPages() && p !== this.pageNumber()) {
      this.pageChange.emit(p);
    }
  }

  goFirst(): void {
    this.goTo(1);
  }

  goLast(): void {
    this.goTo(this.totalPages());
  }

  goPrev(): void {
    this.goTo(this.pageNumber() - 1);
  }

  goNext(): void {
    this.goTo(this.pageNumber() + 1);
  }

  onJumpSubmit(): void {
    this.goTo(this.jumpPage());
  }

  changeSize(size: number | string): void {
    this.pageSizeChange.emit(Number(size));
  }

  toggleSizeMenu(event: Event): void {
    event.stopPropagation();
    this.sizeOpen.update((open) => !open);
  }

  chooseSize(size: number, event: Event): void {
    event.stopPropagation();
    this.sizeOpen.set(false);
    if (size !== this.pageSize()) {
      this.pageSizeChange.emit(size);
    }
  }

  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.sizeOpen.set(false);
    }
  }
}