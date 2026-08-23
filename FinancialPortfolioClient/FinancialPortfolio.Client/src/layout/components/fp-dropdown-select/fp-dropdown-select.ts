import {
  Component,
  ChangeDetectionStrategy,
  ElementRef,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';

export interface FpDropdownSelectOption {
  value: number | string | null;
  label: string;
}

@Component({
  selector: 'app-fp-dropdown-select',
  standalone: true,
  templateUrl: './fp-dropdown-select.html',
  styleUrl: './fp-dropdown-select.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'fp-select',
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class FpDropdownSelect {
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly options = input.required<FpDropdownSelectOption[]>();
  readonly value = input<number | string | null>(null);
  readonly icon = input('bi-list');
  readonly placeholder = input('Select');
  readonly ariaLabel = input('Select');
  readonly disabled = input(false);

  readonly valueChange = output<number | string | null>();

  readonly open = signal(false);

  readonly selectedLabel = computed(() => {
    const current = this.value();
    return this.options().find((option) => option.value === current)?.label ?? this.placeholder();
  });

  toggle(event: Event): void {
    event.stopPropagation();
    if (this.disabled()) return;
    this.open.update((isOpen) => !isOpen);
  }

  choose(option: FpDropdownSelectOption, event: Event): void {
    event.stopPropagation();
    this.valueChange.emit(option.value);
    this.open.set(false);
  }

  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.open.set(false);
    }
  }
}