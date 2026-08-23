import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '../../../environments/environment';
import { LogLevel } from '../../enums/logs/log-level.enum';
import { ClientLogRequest } from '../../models/system-log/client-log-request';

@Injectable({
  providedIn: 'root',
})
export class ClientLoggerService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/systemlog/client`;

  info(message: string, category: string, method?: string): void {
    this.send(LogLevel.Information, category, method, message);
  }

  warning(message: string, category: string, method?: string): void {
    this.send(LogLevel.Warning, category, method, message);
  }

  error(
    message: string,
    error?: unknown,
    category = 'unknown.ts',
    method?: string
  ): void {
    const { exception, stackTrace } = this.extractError(error);
    this.send(LogLevel.Error, category, method, message, exception, stackTrace);
  }

  critical(
    message: string,
    error?: unknown,
    category = 'unknown.ts',
    method?: string
  ): void {
    const { exception, stackTrace } = this.extractError(error);
    this.send(
      LogLevel.Critical,
      category,
      method,
      message,
      exception,
      stackTrace
    );
  }

  audit(message: string, category: string, method?: string): void {
    this.send(LogLevel.Audit, category, method, message);
  }

  security(message: string, category: string, method?: string): void {
    this.send(LogLevel.Security, category, method, message);
  }

  private send(
    level: LogLevel,
    category: string,
    method: string | undefined,
    message: string,
    exception?: string | null,
    stackTrace?: string | null
  ): void {
    const body: ClientLogRequest = {
      level,
      category: category?.trim() || 'unknown.ts',
      method: method?.trim() || undefined,
      message: message?.trim() || '(empty)',
      exception: exception ?? null,
      stackTrace: stackTrace ?? null,
      pageUrl: typeof window !== 'undefined' ? window.location.href : undefined,
      userAgent:
        typeof navigator !== 'undefined' ? navigator.userAgent : undefined,
    };

    // Fire-and-forget — never block UI
    this.http.post(this.apiUrl, body).subscribe({ error: () => {} });
  }

  private extractError(error: unknown): {
    exception: string | null;
    stackTrace: string | null;
  } {
    if (!error) {
      return { exception: null, stackTrace: null };
    }

    if (error instanceof Error) {
      return {
        exception: error.message,
        stackTrace: error.stack ?? error.message,
      };
    }

    if (typeof error === 'string') {
      return { exception: error, stackTrace: error };
    }

    try {
      const text = JSON.stringify(error);
      return { exception: text, stackTrace: text };
    } catch {
      return { exception: String(error), stackTrace: String(error) };
    }
  }
}