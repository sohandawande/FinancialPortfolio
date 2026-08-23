import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { QueryRequest } from '../../models/query/query-request.model';
import { PagedResponse } from '../../models/query/paged-response.model';
import { Stock } from '../../models/stock/stock.model';

export interface StockCreateRequest {
  symbol: string;
  companyName: string;
  industry: string;
  isinCode: string;
  series: string;
  currentPrice: number;
  marketCap: number;
}

export interface StockUpdateRequest {
  symbol: string;
  companyName: string;
  industry: string;
  isinCode: string;
  series: string;
  currentPrice: number;
  marketCap: number;
}

/**
 * Maps to server StockController → api/Stock/*
 */
@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/stock`;

  /** POST api/Stock/search */
  search(request: QueryRequest): Observable<PagedResponse<Stock>> {
    return this.http
      .post<ApiResponse<PagedResponse<Stock> | Stock[]>>(`${this.apiUrl}/search`, request)
      .pipe(
        map((res) => {
          const data = res.data;

          if (Array.isArray(data)) {
            return {
              data,
              totalRecords: data.length,
              totalPages: 1,
              pageNumber: request.pageNumber,
              pageSize: request.pageSize,
            };
          }

          return {
            data: data?.data ?? [],
            totalRecords: data?.totalRecords ?? 0,
            totalPages: data?.totalPages ?? 0,
            pageNumber: data?.pageNumber ?? request.pageNumber,
            pageSize: data?.pageSize ?? request.pageSize,
          };
        }),
        catchError(() =>
          of({
            data: [],
            totalRecords: 0,
            totalPages: 0,
            pageNumber: request.pageNumber,
            pageSize: request.pageSize,
          }),
        ),
      );
  }

  /** GET api/Stock/{id}/get-by-id */
  getById(id: number): Observable<Stock | null> {
    return this.http
      .get<ApiResponse<Stock>>(`${this.apiUrl}/get-by-id/${id}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }

  /** POST api/Stock/create */
  create(request: StockCreateRequest): Observable<Stock | null> {
    return this.http
      .post<ApiResponse<Stock>>(`${this.apiUrl}/create`, request)
      .pipe(
        map((res) => (res.success ? (res.data ?? null) : null)),
        catchError(() => of(null)),
      );
  }

  /** PUT api/Stock/{id}/update */
  update(id: number, request: StockUpdateRequest): Observable<boolean> {
    return this.http
      .put<ApiResponse<boolean>>(`${this.apiUrl}/update/${id}`, request)
      .pipe(
        map((res) => !!res?.success),
        catchError(() => of(false)),
      );
  }

  /** DELETE api/Stock/{id}/delete */
  delete(id: number): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/delete/${id}`)
      .pipe(
        map((res) => !!res?.success),
        catchError(() => of(false)),
      );
  }
}