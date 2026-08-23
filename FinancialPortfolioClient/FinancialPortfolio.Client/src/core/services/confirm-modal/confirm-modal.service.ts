import { Injectable, signal } from '@angular/core';
import { ConfirmModalOptions } from '../../models/confirm-modal/confirm-modal.model';

@Injectable({ providedIn: 'root' })
export class ConfirmModalService {
  readonly visible = signal(false);
  readonly options = signal<ConfirmModalOptions>({ message: '' });

  private resolveFn: ((ok: boolean) => void) | null = null;

  open(options: ConfirmModalOptions): Promise<boolean> {
    this.options.set({
      title: 'Confirm',
      confirmText: 'Yes',
      cancelText: 'Cancel',
      confirmColor: 'primary',
      ...options,
    });
    this.visible.set(true);

    return new Promise<boolean>((resolve) => {
      this.resolveFn = resolve;
    });
  }

  confirm(): void {
    this.visible.set(false);
    this.resolveFn?.(true);
    this.resolveFn = null;
  }

  cancel(): void {
    this.visible.set(false);
    this.resolveFn?.(false);
    this.resolveFn = null;
  }
}