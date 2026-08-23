import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { Portfolio } from '../../models/portfolio/portfolio.model';
import { PortfolioSummary } from '../../models/portfolio/portfolio-summary.model';
import { PortfolioHolding } from '../../models/portfolio/portfolio-holding.model';
import { PortfolioSold } from '../../models/portfolio/portfolio-sold.model';
import { PortfolioLedgerItem } from '../../models/portfolio/portfolio-ledger-item.model';
import { PortfolioLedgerFilter } from '../../models/portfolio/portfolio-ledger-filter.model';
import { PortfolioPosition } from '../../models/portfolio/portfolio-position.model';
import { PortfolioPositionDetail } from '../../models/portfolio/portfolio-position-detail.model';
import { PortfolioPositionFilter } from '../../models/portfolio/portfolio-position-filter.model';
import { PortfolioDividend } from '../../models/portfolio/portfolio-dividend.model';
import { PortfolioDividendOverview } from '../../models/portfolio/portfolio-dividend-overview.model';
import { AddDividendRequest } from '../../models/portfolio/add-dividend-request.model';
import { UpdateDividendRequest } from '../../models/portfolio/update-dividend-request.model';
import { BuyStockRequest } from '../../models/portfolio/buy-stock-request.model';
import { UpdateHoldRequest } from '../../models/portfolio/update-hold-request.model';
import { SellStockRequest } from '../../models/portfolio/sell-stock-request.model';
import { UpdateSoldRequest } from '../../models/portfolio/update-sold-request.model';
import { FifoSellResponse } from '../../models/portfolio/fifo-sell-response.model';
import { CreatePortfolioRequest } from '../../models/portfolio/create-portfolio-request.model';
import { UpdatePortfolioRequest } from '../../models/portfolio/update-portfolio-request.model';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/portfolio`;

  getPortfolio(): Observable<Portfolio | null> {
    return this.http.get<ApiResponse<Portfolio | null>>(this.apiUrl).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null)),
    );
  }

  create(request: CreatePortfolioRequest): Observable<ApiResponse<Portfolio>> {
    return this.http.post<ApiResponse<Portfolio>>(this.apiUrl, request);
  }

  update(request: UpdatePortfolioRequest): Observable<ApiResponse<Portfolio>> {
    return this.http.put<ApiResponse<Portfolio>>(this.apiUrl, request);
  }

  getSummary(): Observable<PortfolioSummary | null> {
    return this.http.get<ApiResponse<PortfolioSummary>>(`${this.apiUrl}/summary`).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null)),
    );
  }

  getHoldings(): Observable<PortfolioHolding[]> {
    return this.http.get<ApiResponse<PortfolioHolding[]>>(`${this.apiUrl}/holdings`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([])),
    );
  }

  getSoldHistory(): Observable<PortfolioSold[]> {
    return this.http.get<ApiResponse<PortfolioSold[]>>(`${this.apiUrl}/sold`).pipe(
      map((res) => res.data ?? []),
      catchError(() => of([])),
    );
  }

  getLedger(type: PortfolioLedgerFilter = 'lifetime'): Observable<PortfolioLedgerItem[]> {
    return this.http
      .get<ApiResponse<PortfolioLedgerItem[]>>(`${this.apiUrl}/ledger`, {
        params: { type },
      })
      .pipe(
        map((res) => res.data ?? []),
        catchError(() => of([])),
      );
  }

  getPositions(status: PortfolioPositionFilter = 'all'): Observable<PortfolioPosition[]> {
    return this.http
      .get<ApiResponse<PortfolioPosition[]>>(`${this.apiUrl}/positions`, {
        params: { status },
      })
      .pipe(
        map((res) => res.data ?? []),
        catchError(() => of([])),
      );
  }

  getPositionDetail(stockId: number): Observable<PortfolioPositionDetail | null> {
    return this.http
      .get<ApiResponse<PortfolioPositionDetail>>(`${this.apiUrl}/positions/${stockId}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }

  getDividends(stockId?: number): Observable<PortfolioDividend[]> {
    const params = stockId ? { stockId } : undefined;
    return this.http
      .get<ApiResponse<PortfolioDividend[]>>(`${this.apiUrl}/dividends`, { params })
      .pipe(
        map((res) => res.data ?? []),
        catchError(() => of([])),
      );
  }

  getDividendOverview(): Observable<PortfolioDividendOverview | null> {
    return this.http
      .get<ApiResponse<PortfolioDividendOverview>>(`${this.apiUrl}/dividends/overview`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null)),
      );
  }

  addDividend(request: AddDividendRequest): Observable<ApiResponse<PortfolioDividend>> {
    return this.http.post<ApiResponse<PortfolioDividend>>(`${this.apiUrl}/dividend`, request);
  }

  updateDividend(
    id: number,
    request: UpdateDividendRequest,
  ): Observable<ApiResponse<PortfolioDividend>> {
    return this.http.put<ApiResponse<PortfolioDividend>>(`${this.apiUrl}/dividend/${id}`, request);
  }

  deleteDividend(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/dividend/${id}`);
  }

  buy(request: BuyStockRequest): Observable<PortfolioHolding | null> {
    return this.http.post<ApiResponse<PortfolioHolding>>(`${this.apiUrl}/buy`, request).pipe(
      map((res) => res.data ?? null),
      catchError(() => of(null)),
    );
  }

  updateHold(id: number, request: UpdateHoldRequest): Observable<ApiResponse<PortfolioHolding>> {
    return this.http.put<ApiResponse<PortfolioHolding>>(`${this.apiUrl}/hold/${id}`, request);
  }

  deleteHold(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/hold/${id}`);
  }

  sell(request: SellStockRequest): Observable<ApiResponse<FifoSellResponse>> {
    return this.http.post<ApiResponse<FifoSellResponse>>(`${this.apiUrl}/sell`, request);
  }

  updateSold(id: number, request: UpdateSoldRequest): Observable<ApiResponse<PortfolioSold>> {
    return this.http.put<ApiResponse<PortfolioSold>>(`${this.apiUrl}/sold/${id}`, request);
  }

  deleteSold(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/sold/${id}`);
  }
}
