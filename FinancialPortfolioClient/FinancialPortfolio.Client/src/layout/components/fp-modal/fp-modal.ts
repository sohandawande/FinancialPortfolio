import {
  Component,
  ChangeDetectionStrategy,
  ViewEncapsulation,
  HostListener,
  input,
  output,
  computed,
  effect,
} from '@angular/core';

import {
  FP_MODAL_WIDTH,
  FpModalSize,
  FpModalVariant,
} from '../../../core/models/fp-modal/fp-modal.model';

@Component({
  selector: 'app-fp-modal',
  standalone: true,
  templateUrl: './fp-modal.html',
  styleUrl: './fp-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class FpModal {
  readonly open = input(false);
  readonly title = input.required<string>();
  readonly subtitle = input('');
  readonly icon = input('bi-window');
  readonly variant = input<FpModalVariant>('default');
  /** Preset: xs 360 · sm 420 · md 520 · lg 720 · xl 920 · full 1100 */
  readonly size = input<FpModalSize>('md');
  /** Optional override, e.g. 480, '32rem', '50%'. Wins over size. */
  readonly width = input<string | number | null>(null);
  readonly closable = input(true);
  readonly closeOnBackdrop = input(true);
  readonly closeOnEscape = input(true);
  readonly busy = input(false);
  readonly showFooter = input(true);
  readonly showConfirm = input(true);
  readonly showCancel = input(true);
  readonly cancelText = input('Cancel');
  readonly confirmText = input('Save');
  readonly confirmIcon = input('');
  readonly confirmColor = input<'primary' | 'danger' | 'success' | 'warning' | 'outline-secondary'>('primary');
  readonly confirmDisabled = input(false);

  readonly closed = output<void>();
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  private static openCount = 0;

  readonly dialogStyle = computed(() => {
    const custom = this.width();
    let value: string;
    if (custom === 0 || custom === '0') {
      value = FP_MODAL_WIDTH[this.size()];
    } else if (typeof custom === 'number') {
      value = `${custom}px`;
    } else if (custom != null && String(custom).trim() !== '') {
      const raw = String(custom).trim();
      value = /^\d+$/.test(raw) ? `${raw}px` : raw;
    } else {
      value = FP_MODAL_WIDTH[this.size()];
    }
    return { '--fp-modal-width': value };
  });

  readonly confirmBtnClass = computed(() => `btn btn-${this.confirmColor()}`);

  constructor() {
    effect((onCleanup) => {
      if (!this.open()) return;
      FpModal.openCount += 1;
      document.body.classList.add('fp-modal-open');
      onCleanup(() => {
        FpModal.openCount = Math.max(0, FpModal.openCount - 1);
        if (FpModal.openCount === 0) {
          document.body.classList.remove('fp-modal-open');
        }
      });
    });
  }

  onBackdrop(): void {
    if (!this.closeOnBackdrop() || this.busy() || !this.closable()) return;
    this.cancel();
  }

  onCloseClick(): void {
    if (this.busy() || !this.closable()) return;
    this.cancel();
  }

  cancel(): void {
    this.cancelled.emit();
    this.closed.emit();
  }

  confirm(): void {
    if (this.busy() || this.confirmDisabled()) return;
    this.confirmed.emit();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (!this.open() || !this.closeOnEscape() || this.busy() || !this.closable()) return;
    this.cancel();
  }
}
