/**
 * Shared stock list formatters (HTML safe for data-grid [innerHTML]).
 */

import { environment } from '../../../environments/environment';

export function formatStockMoney(value: number | null | undefined): string {
  if (value == null || isNaN(Number(value))) return '—';
  return (
    '₹' +
    Number(value).toLocaleString('en-IN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })
  );
}

export function formatStockNumber(value: number | null | undefined): string {
  if (value == null || isNaN(Number(value))) return '—';
  return Number(value).toLocaleString('en-IN');
}

/** Resolve logo URL (absolute http, relative to API host, or ui-avatars fallback). */
function apiHost(): string {
  return environment.apiUrl.replace(/\/api\/?$/, '');
}

export function stockLogoFileUrl(symbol: string | null | undefined): string | null {
  const sym = (symbol || '').trim().toUpperCase().replace(/[^A-Z0-9]/g, '');
  if (!sym) return null;
  return `${apiHost()}/logos/${encodeURIComponent(sym)}.png`;
}

export function resolveStockLogoUrl(
  logoUrl: string | null | undefined,
  symbol: string | null | undefined
): string {
  const raw = logoUrl?.trim();
  if (raw) {
    if (raw.startsWith('http://') || raw.startsWith('https://')) return raw;
    if (raw.startsWith('//')) return `https:${raw}`;
    return `${apiHost()}${raw.startsWith('/') ? '' : '/'}${raw}`;
  }

  return stockLogoFileUrl(symbol) ?? initialsAvatarUrl(symbol);
}

export function initialsAvatarUrl(symbol: string | null | undefined): string {
  const name = encodeURIComponent(
    (symbol || 'ST').replace(/[^A-Za-z0-9]/g, '').slice(0, 2).toUpperCase() || 'ST'
  );
  return `https://ui-avatars.com/api/?name=${name}&background=42b883&color=fff&bold=true&size=128`;
}

/** Symbol only (legacy). Prefer formatStockSymbolWithLogoHtml for grid. */
export function formatStockSymbolHtml(symbol: string | null | undefined): string {
  const text = (symbol ?? '').trim();
  return `<span class="stock-symbol">${text}</span>`;
}

/**
 * Beautiful symbol cell: small circular logo + bold symbol.
 * Uses onerror fallback to ui-avatars so broken logos still look good.
 */
export function formatStockSymbolWithLogoHtml(
  symbol: string | null | undefined,
  logoUrl: string | null | undefined
): string {
  const text = (symbol ?? '').trim();
  const src = resolveStockLogoUrl(logoUrl, text);
  const fallback = initialsAvatarUrl(text);

  return `
    <span class="stock-symbol-cell">
      <img
        class="stock-logo"
        src="${src}"
        alt="${text}"
        loading="lazy"
        onerror="this.onerror=null;this.src='${fallback}'"
      />
      <span class="stock-symbol">${text}</span>
    </span>
  `.trim();
}

export function formatStockCompanyHtml(
  companyName: string | null | undefined
): string {
  return `<span class="stock-company">${companyName ?? ''}</span>`;
}

/** Price with ₹ prefix */
export function formatStockPriceHtml(value: number | null | undefined): string {
  return `<span class="stock-price">${formatStockMoney(value)}</span>`;
}

/** Green / red change pill with arrow */
export function formatStockChangeHtml(value: number | null | undefined): string {
  if (value == null || isNaN(Number(value))) {
    return `<span class="stock-change flat">—</span>`;
  }

  const n = Number(value);
  const cls = n > 0 ? 'up' : n < 0 ? 'down' : 'flat';
  const icon =
    n > 0 ? 'bi-arrow-up-right' : n < 0 ? 'bi-arrow-down-right' : 'bi-dash';
  const sign = n > 0 ? '+' : '';

  return `<span class="stock-change ${cls}">
    <i class="bi ${icon}"></i>
    ${sign}${n.toFixed(2)}
  </span>`;
}

export function formatStockActiveHtml(isActive: boolean | null | undefined): string {
  return isActive
    ? `<span class="badge rounded-pill stock-active-yes">Yes</span>`
    : `<span class="badge rounded-pill stock-active-no">No</span>`;
}

/** Short label + distinct color for Large / Mid / Small / Micro */
const CATEGORY_MAP: Record<string, { label: string; cls: string }> = {
  'large cap': { label: 'Large', cls: 'stock-cat-large' },
  large: { label: 'Large', cls: 'stock-cat-large' },
  'mid cap': { label: 'Mid', cls: 'stock-cat-mid' },
  mid: { label: 'Mid', cls: 'stock-cat-mid' },
  'middle cap': { label: 'Mid', cls: 'stock-cat-mid' },
  'small cap': { label: 'Small', cls: 'stock-cat-small' },
  small: { label: 'Small', cls: 'stock-cat-small' },
  'micro cap': { label: 'Micro', cls: 'stock-cat-micro' },
  micro: { label: 'Micro', cls: 'stock-cat-micro' },
};

export function formatStockCategoryHtml(
  category: string | null | undefined
): string {
  if (!category?.trim()) {
    return `<span class="badge rounded-pill stock-cat-unknown">—</span>`;
  }

  const key = category.trim().toLowerCase().replace(/\s+/g, ' ');
  const mapped = CATEGORY_MAP[key];

  if (mapped) {
    return `<span class="badge rounded-pill ${mapped.cls}">${mapped.label}</span>`;
  }

  // Fallback: show original text (trimmed) in neutral badge
  return `<span class="badge rounded-pill stock-cat-unknown">${category.trim()}</span>`;
}