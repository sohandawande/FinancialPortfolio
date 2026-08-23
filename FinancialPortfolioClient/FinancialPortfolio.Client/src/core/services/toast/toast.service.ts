import { Injectable, signal } from '@angular/core';
import { ToastMessage, ToastType } from '../../models/toast/toast.model';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private seq = 0;
  readonly toasts = signal<ToastMessage[]>([]);

  success(message: string, title = 'Success', delay = 3500): void {
    this.push('success', message, title, delay);
  }

  error(message: string, title = 'Error', delay = 5000): void {
    this.push('danger', message, title, delay);
  }

  warning(message: string, title = 'Warning', delay = 4000): void {
    this.push('warning', message, title, delay);
  }

  info(message: string, title = 'Info', delay = 3500): void {
    this.push('info', message, title, delay);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  clear(): void {
    this.toasts.set([]);
  }

  private push(type: ToastType, message: string, title: string, delay: number): void {
    const id = ++this.seq;
    this.toasts.update((list) => [...list, { id, type, title, message, delay }]);
    window.setTimeout(() => this.dismiss(id), delay);
  }
}