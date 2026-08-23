import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { PendingUser } from '../../models/user/pending-user.model';
import { AssignRoleRequest } from '../../models/user/assign-role-request';
import { ManagedUser } from '../../models/user/managed-user.model';
import { UserDetail } from '../../models/user/user-detail.model';

/**
 * Maps to server AppUserController → api/AppUser/*
 */
@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/appuser`;

  /** GET api/AppUser/pending */
  getPendingUsers(): Observable<PendingUser[]> {
    return this.http.get<ApiResponse<PendingUser[]>>(`${this.apiUrl}/pending`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([])),
    );
  }

  /** GET api/AppUser/manage-users */
  getManagedUsers(): Observable<ManagedUser[]> {
    return this.http
      .get<ApiResponse<ManagedUser[]>>(`${this.apiUrl}/manage-users`)
      .pipe(
        map((res) => res.data ?? []),
        catchError(() => of([])),
      );
  }

  /** GET api/AppUser/{identityUserId} */
  getUserById(identityUserId: string): Observable<UserDetail | null> {
    return this.http
      .get<ApiResponse<UserDetail>>(`${this.apiUrl}/get-user-by-id/${identityUserId}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }

  /** PUT api/AppUser/{identityUserId}/approve */
  approveUser(identityUserId: string, roles: string[]): Observable<boolean> {
    const body: AssignRoleRequest = { roles };
    return this.http
      .put<ApiResponse<boolean>>(`${this.apiUrl}/approve/${identityUserId}`, body)
      .pipe(
        map((res) => !!res?.success && !!res?.data),
        catchError(() => of(false)),
      );
  }

  /** PUT api/AppUser/{identityUserId}/roles */
  assignRoles(identityUserId: string, roles: string[]): Observable<boolean> {
    const body: AssignRoleRequest = { roles };
    return this.http
      .put<ApiResponse<boolean>>(`${this.apiUrl}/roles/${identityUserId}`, body)
      .pipe(
        map((res) => !!res?.success && !!res?.data),
        catchError(() => of(false)),
      );
  }

  /** PUT api/AppUser/{identityUserId}/activate */
  activateUser(identityUserId: string): Observable<boolean> {
    return this.http
      .put<ApiResponse<boolean>>(`${this.apiUrl}/activate/${identityUserId}`, {})
      .pipe(
        map((res) => !!res?.success && !!res?.data),
        catchError(() => of(false)),
      );
  }

  /** PUT api/AppUser/{identityUserId}/deactivate */
  deactivateUser(identityUserId: string): Observable<boolean> {
    return this.http
      .put<ApiResponse<boolean>>(`${this.apiUrl}/deactivate/${identityUserId}`, {})
      .pipe(
        map((res) => !!res?.success && !!res?.data),
        catchError(() => of(false)),
      );
  }
}