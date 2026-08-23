import { LogLevel } from '../../enums/logs/log-level.enum';

export interface ClientLogRequest {
  level: LogLevel;
  category: string;
  method?: string;
  message: string;
  exception?: string | null;
  stackTrace?: string | null;
  pageUrl?: string;
  userAgent?: string;
}