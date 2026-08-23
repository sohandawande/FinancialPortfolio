export type ToastType = 'success' | 'danger' | 'warning' | 'info';

export interface ToastMessage {
  id: number;
  type: ToastType;
  title?: string;
  message: string;
  delay: number;
}