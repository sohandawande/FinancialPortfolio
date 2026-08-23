import {
  Component,
  ElementRef,
  HostListener,
  computed,
  forwardRef,
  inject,
  input,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

const WEEKDAYS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];
const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

export function todayIso(): string {
  const d = new Date();
  return toIso(d.getFullYear(), d.getMonth() + 1, d.getDate());
}

function toIso(y: number, m: number, d: number): string {
  return `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
}

function parseIso(value: string | null | undefined): { y: number; m: number; d: number } | null {
  if (!value) return null;
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value.trim());
  if (!m) return null;
  const y = Number(m[1]);
  const mo = Number(m[2]);
  const da = Number(m[3]);
  const dt = new Date(y, mo - 1, da);
  if (dt.getFullYear() !== y || dt.getMonth() !== mo - 1 || dt.getDate() !== da) return null;
  return { y, m: mo, d: da };
}

function parseLoose(raw: string): string | null {
  const t = raw.trim();
  if (!t) return null;
  const iso = parseIso(t);
  if (iso) return toIso(iso.y, iso.m, iso.d);
  const dmy = /^(\d{1,2})[-/.](\d{1,2})[-/.](\d{4})$/.exec(t);
  if (dmy) return toIso(Number(dmy[3]), Number(dmy[2]), Number(dmy[1]));
  return null;
}

function display(iso: string): string {
  const p = parseIso(iso);
  if (!p) return '';
  return `${String(p.d).padStart(2, '0')}-${String(p.m).padStart(2, '0')}-${p.y}`;
}

interface CalDay {
  iso: string;
  day: number;
  outside: boolean;
  disabled: boolean;
  today: boolean;
  selected: boolean;
}

@Component({
  selector: 'app-fp-date',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fp-date.html',
  styleUrl: './fp-date.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FpDate),
      multi: true,
    },
  ],
})
export class FpDate implements ControlValueAccessor {
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly max = input<string | null>(todayIso());
  readonly min = input<string | null>(null);
  readonly placeholder = input('dd-mm-yyyy');
  readonly locked = input(false, { alias: 'disabled' });

  readonly open = signal(false);
  readonly cvaDisabled = signal(false);
  readonly disabled = computed(() => this.locked() || this.cvaDisabled());
  readonly iso = signal('');
  readonly typed = signal('');
  readonly viewYear = signal(new Date().getFullYear());
  readonly viewMonth = signal(new Date().getMonth());
  readonly viewMode = signal<'days' | 'months' | 'years'>('days');
  readonly decadeStart = signal(Math.floor(new Date().getFullYear() / 12) * 12);
  readonly weekdays = WEEKDAYS;
  readonly months = MONTHS;
  readonly panel = signal({ top: 0, left: 0 });

  readonly monthLabel = computed(() => MONTHS[this.viewMonth()]);
  readonly yearLabel = computed(() => String(this.viewYear()));
  readonly decadeLabel = computed(() => `${this.decadeStart()} – ${this.decadeStart() + 11}`);

  readonly monthChoices = computed(() => {
    const y = this.viewYear();
    const max = this.max();
    const min = this.min();
    return MONTHS.map((label, index) => {
      const start = toIso(y, index + 1, 1);
      const end = toIso(y, index + 1, new Date(y, index + 1, 0).getDate());
      const afterMax = Boolean(max && start > max);
      const beforeMin = Boolean(min && end < min);
      return { index, label: label.slice(0, 3), disabled: afterMax || beforeMin };
    });
  });

  readonly yearChoices = computed(() => {
    const start = this.decadeStart();
    const max = this.max();
    const min = this.min();
    const maxY = max ? Number(max.slice(0, 4)) : null;
    const minY = min ? Number(min.slice(0, 4)) : null;
    return Array.from({ length: 12 }, (_, i) => {
      const year = start + i;
      return {
        year,
        disabled: Boolean((maxY != null && year > maxY) || (minY != null && year < minY)),
      };
    });
  });

  readonly days = computed<CalDay[]>(() => {
    const y = this.viewYear();
    const m = this.viewMonth();
    const first = new Date(y, m, 1);
    const start = first.getDay();
    const lastDate = new Date(y, m + 1, 0).getDate();
    const prevLast = new Date(y, m, 0).getDate();
    const selected = this.iso();
    const today = todayIso();
    const max = this.max();
    const min = this.min();
    const cells: CalDay[] = [];

    for (let i = 0; i < 42; i++) {
      let yy = y;
      let mm = m;
      let dd: number;
      let outside = false;
      if (i < start) {
        dd = prevLast - start + i + 1;
        mm = m - 1;
        if (mm < 0) {
          mm = 11;
          yy -= 1;
        }
        outside = true;
      } else if (i >= start + lastDate) {
        dd = i - start - lastDate + 1;
        mm = m + 1;
        if (mm > 11) {
          mm = 0;
          yy += 1;
        }
        outside = true;
      } else {
        dd = i - start + 1;
      }
      const iso = toIso(yy, mm + 1, dd);
      const disabled = Boolean((max && iso > max) || (min && iso < min));
      cells.push({
        iso,
        day: dd,
        outside,
        disabled,
        today: iso === today,
        selected: iso === selected,
      });
    }
    return cells;
  });

  private onChange: (v: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    const next = value ? parseLoose(value) ?? '' : '';
    this.iso.set(next);
    this.typed.set(next ? display(next) : '');
    if (next) {
      const p = parseIso(next);
      if (p) {
        this.viewYear.set(p.y);
        this.viewMonth.set(p.m - 1);
      }
    }
  }

  registerOnChange(fn: (v: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }

  toggle(): void {
    if (this.disabled()) return;
    if (this.open()) {
      this.close();
      return;
    }
    this.placePanel();
    const p = parseIso(this.iso());
    if (p) {
      this.viewYear.set(p.y);
      this.viewMonth.set(p.m - 1);
    }
    this.viewMode.set('days');
    this.decadeStart.set(Math.floor(this.viewYear() / 12) * 12);
    this.open.set(true);
  }

  close(): void {
    this.open.set(false);
    this.viewMode.set('days');
    this.onTouched();
  }

  showMonths(): void {
    this.viewMode.set('months');
  }

  showYears(): void {
    this.decadeStart.set(Math.floor(this.viewYear() / 12) * 12);
    this.viewMode.set('years');
  }

  pickMonth(index: number, disabled: boolean): void {
    if (disabled) return;
    this.viewMonth.set(index);
    this.viewMode.set('days');
  }

  pickYear(year: number, disabled: boolean): void {
    if (disabled) return;
    this.viewYear.set(year);
    const max = this.max();
    if (max) {
      const maxY = Number(max.slice(0, 4));
      const maxM = Number(max.slice(5, 7)) - 1;
      if (year === maxY && this.viewMonth() > maxM) this.viewMonth.set(maxM);
    }
    this.viewMode.set('months');
  }

  prevDecade(): void {
    this.decadeStart.update((s) => s - 12);
  }

  nextDecade(): void {
    const max = this.max();
    const next = this.decadeStart() + 12;
    if (max && next > Number(max.slice(0, 4))) return;
    this.decadeStart.set(next);
  }

  pick(day: CalDay): void {
    if (day.disabled) return;
    this.commit(day.iso);
    this.close();
  }

  prevMonth(): void {
    if (this.viewMode() === 'years') {
      this.prevDecade();
      return;
    }
    if (this.viewMode() === 'months') {
      this.viewYear.update((y) => y - 1);
      return;
    }
    if (this.viewMonth() === 0) {
      this.viewMonth.set(11);
      this.viewYear.update((y) => y - 1);
    } else {
      this.viewMonth.update((m) => m - 1);
    }
  }

  nextMonth(): void {
    if (this.viewMode() === 'years') {
      this.nextDecade();
      return;
    }
    if (this.viewMode() === 'months') {
      const max = this.max();
      if (max && this.viewYear() + 1 > Number(max.slice(0, 4))) return;
      this.viewYear.update((y) => y + 1);
      return;
    }
    const nextM = this.viewMonth() === 11 ? 0 : this.viewMonth() + 1;
    const nextY = this.viewMonth() === 11 ? this.viewYear() + 1 : this.viewYear();
    const max = this.max();
    if (max) {
      const mp = parseIso(max);
      if (mp && (nextY > mp.y || (nextY === mp.y && nextM > mp.m - 1))) return;
    }
    this.viewMonth.set(nextM);
    this.viewYear.set(nextY);
  }

  goToday(): void {
    const today = todayIso();
    const max = this.max();
    if (max && today > max) return;
    const p = parseIso(today)!;
    this.viewYear.set(p.y);
    this.viewMonth.set(p.m - 1);
    this.commit(today);
    this.close();
  }

  onTyped(event: Event): void {
    const raw = (event.target as HTMLInputElement).value;
    this.typed.set(raw);
  }

  onBlur(): void {
    const parsed = parseLoose(this.typed());
    if (!parsed) {
      this.typed.set(this.iso() ? display(this.iso()) : '');
      this.onTouched();
      return;
    }
    this.commit(this.clamp(parsed));
    this.onTouched();
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') this.close();
    if (event.key === 'Enter') {
      event.preventDefault();
      this.onBlur();
      this.close();
    }
  }

  @HostListener('document:click', ['$event'])
  onDocClick(event: MouseEvent): void {
    if (!this.open()) return;
    if (!this.host.nativeElement.contains(event.target as Node)) this.close();
  }

  private commit(iso: string): void {
    const next = this.clamp(iso);
    this.iso.set(next);
    this.typed.set(display(next));
    this.onChange(next);
  }

  private clamp(iso: string): string {
    const max = this.max();
    const min = this.min();
    if (max && iso > max) return max;
    if (min && iso < min) return min;
    return iso;
  }

  private placePanel(): void {
    const trigger = this.host.nativeElement.querySelector('.fp-date-trigger') as HTMLElement | null;
    const rect = (trigger ?? this.host.nativeElement).getBoundingClientRect();
    const width = 280;
    const left = Math.min(rect.left, window.innerWidth - width - 12);
    const top = rect.bottom + 6;
    this.panel.set({ top, left: Math.max(12, left) });
  }
}
