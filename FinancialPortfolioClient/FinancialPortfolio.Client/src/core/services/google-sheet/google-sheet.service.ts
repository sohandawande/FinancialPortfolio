import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import {
  GoogleSheetSyncRequest,
  GoogleSheetSyncResponse,
} from '../../models/google-sheet/google-sheet-sync.model';

@Injectable({ providedIn: 'root' })
export class GoogleSheetService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/googlesheet`;

  syncStocks(forceRefresh = true): Observable<GoogleSheetSyncResponse | null> {
    const body: GoogleSheetSyncRequest = { forceRefresh };

    return this.http
      .post<ApiResponse<GoogleSheetSyncResponse>>(`${this.apiUrl}/sync`, body)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null))
      );
  }
}