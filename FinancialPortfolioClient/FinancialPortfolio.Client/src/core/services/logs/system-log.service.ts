import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { QueryRequest } from '../../models/query/query-request.model';
import { PagedResponse } from '../../models/query/paged-response.model';
import { SystemLog } from '../../models/system-log/system-log.model';

/**
 * Maps to server SystemLogController → api/SystemLog/*
 */
@Injectable({ providedIn: 'root' })
export class SystemLogService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/systemlog`;

  /** POST api/SystemLog/get-all */
  getAll(request: QueryRequest): Observable<PagedResponse<SystemLog>> {
    return this.http
      .post<ApiResponse<PagedResponse<SystemLog> | SystemLog[]>>(
        `${this.apiUrl}/get-all`,
        request,
      )
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

  /** GET api/SystemLog/{id}/get-by-id */
  getById(id: number): Observable<SystemLog | null> {
    return this.http
      .get<ApiResponse<SystemLog>>(`${this.apiUrl}/get-by-id/${id}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }
}