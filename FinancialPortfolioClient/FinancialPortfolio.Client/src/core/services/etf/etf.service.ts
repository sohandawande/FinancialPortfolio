import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { QueryRequest } from '../../models/query/query-request.model';
import { PagedResponse } from '../../models/query/paged-response.model';
import { Etf } from '../../models/etf/etf.model';

export interface EtfCreateRequest {
  symbol: string;
  companyName: string;
  industry: string;
  isinCode: string;
  series: string;
  currentPrice: number;
  marketCap: number;
}

export interface EtfUpdateRequest {
  symbol: string;
  companyName: string;
  industry: string;
  isinCode: string;
  series: string;
  currentPrice: number;
  marketCap: number;
}

/**
 * Maps to server EtfController → api/Etf/*
 */
@Injectable({ providedIn: 'root' })
export class EtfService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/etf`;

  /** POST api/Etf/search */
  search(request: QueryRequest): Observable<PagedResponse<Etf>> {
    return this.http
      .post<ApiResponse<PagedResponse<Etf> | Etf[]>>(`${this.apiUrl}/search`, request)
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

  /** GET api/Etf/{id}/get-by-id */
  getById(id: number): Observable<Etf | null> {
    return this.http
      .get<ApiResponse<Etf>>(`${this.apiUrl}/get-by-id/${id}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }

  /** POST api/Etf/create */
  create(request: EtfCreateRequest): Observable<Etf | null> {
    return this.http
      .post<ApiResponse<Etf>>(`${this.apiUrl}/create`, request)
      .pipe(
        map((res) => (res.success ? (res.data ?? null) : null)),
        catchError(() => of(null)),
      );
  }

  /** PUT api/Etf/{id}/update */
  update(id: number, request: EtfUpdateRequest): Observable<boolean> {
    return this.http
      .put<ApiResponse<boolean>>(`${this.apiUrl}/update/${id}`, request)
      .pipe(
        map((res) => !!res?.success),
        catchError(() => of(false)),
      );
  }

  /** DELETE api/Etf/{id}/delete */
  delete(id: number): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/delete/${id}`)
      .pipe(
        map((res) => !!res?.success),
        catchError(() => of(false)),
      );
  }
}