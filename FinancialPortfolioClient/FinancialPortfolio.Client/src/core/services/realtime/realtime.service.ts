import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

import { environment } from '../../../environments/environment';
import { TokenService } from '../auth/token.service';
import { PendingUser } from '../../models/user/pending-user.model';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly tokenService = inject(TokenService);
  private connection?: signalR.HubConnection;
  private starting?: Promise<void>;

  private readonly pendingUserCreated$ = new Subject<PendingUser>();
  readonly onPendingUserCreated = this.pendingUserCreated$.asObservable();

  private get hubUrl(): string {
    // Prefer explicit hubUrl; else derive from apiUrl
    const anyEnv = environment as { hubUrl?: string; apiUrl: string };
    if (anyEnv.hubUrl) {
      return anyEnv.hubUrl;
    }
    return anyEnv.apiUrl.replace(/\/api\/?$/, '') + '/hubs/notifications';
  }

  async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    // Deduplicate concurrent starts
    if (this.starting) {
      return this.starting;
    }

    const token = this.tokenService.getAccessToken();
    if (!token) {
      return;
    }

    this.starting = this.connect();
    try {
      await this.starting;
    } finally {
      this.starting = undefined;
    }
  }

  private async connect(): Promise<void> {
    // Tear down previous connection if any
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch {
        /* ignore */
      }
      this.connection = undefined;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.tokenService.getAccessToken() ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('PendingUserCreated', (user: PendingUser) => {
      console.log('[SignalR] PendingUserCreated', user);
      this.pendingUserCreated$.next(user);
    });

    this.connection.onreconnected((id) => {
      console.log('[SignalR] reconnected', id);
    });

    this.connection.onclose((err) => {
      console.warn('[SignalR] closed', err);
    });

    try {
      await this.connection.start();
      console.log('[SignalR] connected →', this.hubUrl);
    } catch (err) {
      console.error('[SignalR] connect failed', err);
      this.connection = undefined;
      throw err;
    }
  }

  async stop(): Promise<void> {
    if (!this.connection) {
      return;
    }

    try {
      await this.connection.stop();
    } catch {
      /* ignore */
    }

    this.connection = undefined;
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}