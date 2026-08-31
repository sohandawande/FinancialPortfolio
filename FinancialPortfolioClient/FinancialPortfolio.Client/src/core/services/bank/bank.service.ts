import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, catchError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { BankIfscInfo, BankSuggestion } from '../../models/wealth/wealth.models';

/**
 * Local typeahead list — optional helper only.
 * User can ALWAYS type any bank name manually and save.
 * Add more entries here anytime without backend changes.
 */
const POPULAR_BANKS: BankSuggestion[] = [
  // Public sector
  { name: 'State Bank of India', code: 'SBIN' },
  { name: 'Punjab National Bank', code: 'PUNB' },
  { name: 'Bank of Baroda', code: 'BARB' },
  { name: 'Canara Bank', code: 'CNRB' },
  { name: 'Union Bank of India', code: 'UBIN' },
  { name: 'Bank of India', code: 'BKID' },
  { name: 'Indian Bank', code: 'IDIB' },
  { name: 'Indian Overseas Bank', code: 'IOBA' },
  { name: 'Central Bank of India', code: 'CBIN' },
  { name: 'UCO Bank', code: 'UCBA' },
  { name: 'Bank of Maharashtra', code: 'MAHB' },
  { name: 'Punjab & Sind Bank', code: 'PSIB' },

  // Private
  { name: 'HDFC Bank', code: 'HDFC' },
  { name: 'ICICI Bank', code: 'ICIC' },
  { name: 'Axis Bank', code: 'UTIB' },
  { name: 'Kotak Mahindra Bank', code: 'KKBK' },
  { name: 'Yes Bank', code: 'YESB' },
  { name: 'IDFC FIRST Bank', code: 'IDFB' },
  { name: 'IndusInd Bank', code: 'INDB' },
  { name: 'Federal Bank', code: 'FDRL' },
  { name: 'South Indian Bank', code: 'SIBL' },
  { name: 'RBL Bank', code: 'RATN' },
  { name: 'Bandhan Bank', code: 'BDBL' },
  { name: 'IDBI Bank', code: 'IBKL' },
  { name: 'Jammu & Kashmir Bank', code: 'JAKA' },
  { name: 'Karnataka Bank', code: 'KARB' },
  { name: 'Karur Vysya Bank', code: 'KVBL' },
  { name: 'City Union Bank', code: 'CIUB' },
  { name: 'CSB Bank', code: 'CSBK' },
  { name: 'DCB Bank', code: 'DCBL' },
  { name: 'Dhanlaxmi Bank', code: 'DLXB' },
  { name: 'Nainital Bank', code: 'NTBL' },
  { name: 'Tamilnad Mercantile Bank', code: 'TMBL' },

  // Small finance / payments / others
  { name: 'Utkarsh Small Finance Bank', code: 'UTKS' },
  { name: 'AU Small Finance Bank', code: 'AUBL' },
  { name: 'Equitas Small Finance Bank', code: 'ESFB' },
  { name: 'Ujjivan Small Finance Bank', code: 'UJVN' },
  { name: 'Jana Small Finance Bank', code: 'JSFB' },
  { name: 'Suryoday Small Finance Bank', code: 'SURY' },
  { name: 'ESAF Small Finance Bank', code: 'ESMF' },
  { name: 'Capital Small Finance Bank', code: 'CLBL' },
  { name: 'Fincare Small Finance Bank', code: 'FSFB' },
  { name: 'North East Small Finance Bank', code: 'NESF' },
  { name: 'Shivalik Small Finance Bank', code: 'SMCB' },
  { name: 'Unity Small Finance Bank', code: 'UNBA' },
  { name: 'Airtel Payments Bank', code: 'AIRP' },
  { name: 'India Post Payments Bank', code: 'IPOS' },
  { name: 'Paytm Payments Bank', code: 'PYTM' },
  { name: 'NSDL Payments Bank', code: 'NSPB' },
  { name: 'Fino Payments Bank', code: 'FINO' },
];

@Injectable({ providedIn: 'root' })
export class BankService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/bank`;

  /**
   * Typeahead helper only.
   * Manual free-text bank names are always allowed on the form.
   */
  search(q: string): Observable<BankSuggestion[]> {
    const local = this.filterLocal(q);

    return this.http
      .get<ApiResponse<BankSuggestion[]>>(`${this.apiUrl}/search`, { params: { q: q || '' } })
      .pipe(
        map((r) => this.merge(r.data ?? [], local)),
        catchError(() => of(local)),
      );
  }

  lookupIfsc(ifsc: string): Observable<BankIfscInfo | null> {
    const code = (ifsc || '').trim().toUpperCase();
    if (code.length !== 11) return of(null);

    return this.http.get<ApiResponse<BankIfscInfo | null>>(`${this.apiUrl}/ifsc/${code}`).pipe(
      map((r) => r.data ?? null),
      catchError(() => of(null)),
    );
  }

  private filterLocal(q: string): BankSuggestion[] {
    const term = (q || '').trim().toLowerCase();
    if (!term) return POPULAR_BANKS.slice(0, 12);

    return POPULAR_BANKS.filter(
      (b) =>
        b.name.toLowerCase().includes(term) ||
        (b.code ?? '').toLowerCase().includes(term),
    ).slice(0, 20);
  }

  private merge(api: BankSuggestion[], local: BankSuggestion[]): BankSuggestion[] {
    const seen = new Set<string>();
    const result: BankSuggestion[] = [];

    for (const item of [...api, ...local]) {
      const key = item.name.toLowerCase();
      if (seen.has(key)) continue;
      seen.add(key);
      result.push(item);
      if (result.length >= 20) break;
    }

    return result;
  }
}
