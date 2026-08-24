import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import {
  MarketDataSyncRequest,
  MarketDataSyncResponse,
} from '../../models/market-data/market-data-sync.model';

@Injectable({ providedIn: 'root' })
export class MarketDataService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/marketdata`;

  sync(forceRefresh = true): Observable<MarketDataSyncResponse | null> {
    const body: MarketDataSyncRequest = { forceRefresh };

    return this.http
      .post<ApiResponse<MarketDataSyncResponse>>(`${this.apiUrl}/sync`, body)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }
}
