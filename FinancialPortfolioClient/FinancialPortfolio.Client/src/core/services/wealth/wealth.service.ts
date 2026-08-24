import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import {
  FixedDeposit,
  MutualFund,
  MutualFundNavSync,
  MutualFundSchemeLookup,
  RecurringDeposit,
  UpsertFixedDepositRequest,
  UpsertMutualFundRequest,
  UpsertRecurringDepositRequest,
  WealthSummary,
} from '../../models/wealth/wealth.models';

@Injectable({ providedIn: 'root' })
export class WealthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/wealth`;

  summary(): Observable<WealthSummary> {
    return this.http.get<ApiResponse<WealthSummary>>(`${this.apiUrl}/summary`).pipe(map((r) => r.data));
  }

  mutualFunds(): Observable<MutualFund[]> {
    return this.http.get<ApiResponse<MutualFund[]>>(`${this.apiUrl}/mutual-funds`).pipe(map((r) => r.data ?? []));
  }

  addMutualFund(body: UpsertMutualFundRequest): Observable<ApiResponse<MutualFund>> {
    return this.http.post<ApiResponse<MutualFund>>(`${this.apiUrl}/mutual-funds`, body);
  }

  updateMutualFund(id: number, body: UpsertMutualFundRequest): Observable<ApiResponse<MutualFund>> {
    return this.http.put<ApiResponse<MutualFund>>(`${this.apiUrl}/mutual-funds/${id}`, body);
  }

  deleteMutualFund(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/mutual-funds/${id}`);
  }

  searchSchemes(q: string): Observable<MutualFundSchemeLookup[]> {
    return this.http
      .get<ApiResponse<MutualFundSchemeLookup[]>>(`${this.apiUrl}/mutual-funds/search`, { params: { q } })
      .pipe(map((r) => r.data ?? []));
  }

  syncNav(): Observable<ApiResponse<MutualFundNavSync>> {
    return this.http.post<ApiResponse<MutualFundNavSync>>(`${this.apiUrl}/mutual-funds/sync-nav`, {});
  }

  syncOneNav(id: number): Observable<ApiResponse<MutualFundNavSync>> {
    return this.http.post<ApiResponse<MutualFundNavSync>>(`${this.apiUrl}/mutual-funds/${id}/sync-nav`, {});
  }

  fixedDeposits(): Observable<FixedDeposit[]> {
    return this.http.get<ApiResponse<FixedDeposit[]>>(`${this.apiUrl}/fixed-deposits`).pipe(map((r) => r.data ?? []));
  }

  addFixedDeposit(body: UpsertFixedDepositRequest): Observable<ApiResponse<FixedDeposit>> {
    return this.http.post<ApiResponse<FixedDeposit>>(`${this.apiUrl}/fixed-deposits`, body);
  }

  updateFixedDeposit(id: number, body: UpsertFixedDepositRequest): Observable<ApiResponse<FixedDeposit>> {
    return this.http.put<ApiResponse<FixedDeposit>>(`${this.apiUrl}/fixed-deposits/${id}`, body);
  }

  deleteFixedDeposit(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/fixed-deposits/${id}`);
  }

  recurringDeposits(): Observable<RecurringDeposit[]> {
    return this.http.get<ApiResponse<RecurringDeposit[]>>(`${this.apiUrl}/recurring-deposits`).pipe(map((r) => r.data ?? []));
  }

  addRecurringDeposit(body: UpsertRecurringDepositRequest): Observable<ApiResponse<RecurringDeposit>> {
    return this.http.post<ApiResponse<RecurringDeposit>>(`${this.apiUrl}/recurring-deposits`, body);
  }

  updateRecurringDeposit(id: number, body: UpsertRecurringDepositRequest): Observable<ApiResponse<RecurringDeposit>> {
    return this.http.put<ApiResponse<RecurringDeposit>>(`${this.apiUrl}/recurring-deposits/${id}`, body);
  }

  deleteRecurringDeposit(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/recurring-deposits/${id}`);
  }
}
