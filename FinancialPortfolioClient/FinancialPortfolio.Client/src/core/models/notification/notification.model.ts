export interface AppNotification {
  id: number;
  title: string;
  message: string;
  time: string;
  read: boolean;
  icon?: string;
}