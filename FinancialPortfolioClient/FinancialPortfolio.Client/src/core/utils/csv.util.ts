export type CsvImportKind = 'buy' | 'sell' | 'dividend';

export interface CsvImportRow {
  symbol: string;
  quantity: number;
  price: number;
  amount: number;
  date: string;
  exDate?: string;
  recordDate?: string;
  notes?: string;
}

export function parseCsvLine(line: string): string[] {
  const result: string[] = [];
  let current = '';
  let inQuotes = false;

  for (const char of line) {
    if (char === '"') inQuotes = !inQuotes;
    else if (char === ',' && !inQuotes) {
      result.push(current.trim().replace(/^"|"$/g, ''));
      current = '';
    } else current += char;
  }

  result.push(current.trim().replace(/^"|"$/g, ''));
  return result;
}

export function csvLines(text: string): string[] {
  return text.split(/\r?\n/).filter((l) => l.trim());
}

export function csvHeaders(line: string): string[] {
  return parseCsvLine(line).map((h) => h.toLowerCase());
}

export function headerIndex(headers: string[], ...needles: string[]): number {
  return headers.findIndex((h) => needles.some((n) => h.includes(n)));
}

export function todayIsoDate(): string {
  return new Date().toISOString().substring(0, 10);
}

export function parseImportCsv(
  text: string,
  kind: CsvImportKind,
): { rows: CsvImportRow[]; errors: string[] } {
  const lines = csvLines(text);
  if (lines.length < 2) {
    return { rows: [], errors: ['CSV must have a header and at least one row'] };
  }

  const headers = csvHeaders(lines[0]);
  const symbolIdx = headerIndex(headers, 'symbol');
  const qtyIdx = headerIndex(headers, 'qty', 'quantity', 'shares');
  const exIdx = headerIndex(headers, 'exdate', 'ex date', 'ex-date');
  const recordIdx = headerIndex(headers, 'recorddate', 'record date', 'record-date');
  const notesIdx = headerIndex(headers, 'note', 'remark', 'comment');
  const dateIdx = headers.findIndex((h, i) => {
    if (!h.includes('date')) return false;
    if (i === exIdx || i === recordIdx) return false;
    return true;
  });
  const priceIdx =
    kind === 'sell'
      ? headerIndex(headers, 'sell', 'price')
      : kind === 'buy'
        ? headerIndex(headers, 'buy', 'purchase', 'price')
        : headerIndex(headers, 'pershare', 'per share', 'dps', 'price');
  const amountIdx = headerIndex(headers, 'amount', 'total');

  const missing: string[] = [];
  if (symbolIdx === -1) missing.push('Symbol');
  if (kind === 'buy' && (qtyIdx === -1 || priceIdx === -1)) missing.push('Quantity', 'Purchase Price');
  if (kind === 'buy' && dateIdx === -1) missing.push('Purchase Date');
  if (kind === 'sell' && (qtyIdx === -1 || priceIdx === -1)) missing.push('Quantity', 'Sell Price');
  if (kind === 'sell' && dateIdx === -1) missing.push('Sold Date');
  if (kind === 'dividend' && qtyIdx === -1) missing.push('Quantity');
  if (kind === 'dividend' && amountIdx === -1 && priceIdx === -1) missing.push('Amount or Per Share');
  if (kind === 'dividend' && dateIdx === -1) missing.push('Dividend Date');

  if (missing.length > 0) {
    return { rows: [], errors: [`CSV must contain ${[...new Set(missing)].join(', ')}`] };
  }

  const rows: CsvImportRow[] = [];
  const errors: string[] = [];

  for (let i = 1; i < lines.length; i++) {
    const cols = parseCsvLine(lines[i]);
    const symbol = (cols[symbolIdx] ?? '').trim().toUpperCase();
    const quantity = qtyIdx >= 0 ? Number(cols[qtyIdx]) : NaN;
    const price = priceIdx >= 0 ? Number(cols[priceIdx]) : 0;
    const amount = amountIdx >= 0 ? Number(cols[amountIdx]) : 0;
    const date = dateIdx >= 0 ? (cols[dateIdx] ?? '').trim() : '';
    const exDate = exIdx >= 0 ? (cols[exIdx] ?? '').trim() : '';
    const recordDate = recordIdx >= 0 ? (cols[recordIdx] ?? '').trim() : '';
    const notes = notesIdx >= 0 ? (cols[notesIdx] ?? '').trim() : '';

    if (!symbol) {
      errors.push(`Row ${i + 1}: missing symbol`);
      continue;
    }

    if (kind === 'buy' && !date) {
      errors.push(`Row ${i + 1}: ${symbol} needs Purchase Date`);
      continue;
    }

    if (kind === 'sell' && !date) {
      errors.push(`Row ${i + 1}: ${symbol} needs Sold Date`);
      continue;
    }

    if (kind === 'dividend') {
      if (isNaN(quantity) || quantity <= 0) {
        errors.push(`Row ${i + 1}: ${symbol} needs Quantity`);
        continue;
      }
      const qty = quantity;
      const total = amount > 0 ? amount : price > 0 ? qty * price : 0;
      const perShare = price > 0 ? price : qty > 0 ? total / qty : 0;
      if (total <= 0 && perShare <= 0) {
        errors.push(`Row ${i + 1}: ${symbol} needs Amount or Per Share`);
        continue;
      }
      if (!date) {
        errors.push(`Row ${i + 1}: ${symbol} needs Dividend Date`);
        continue;
      }
      rows.push({
        symbol,
        quantity: qty,
        price: perShare,
        amount: total > 0 ? total : qty * perShare,
        date,
        exDate: exDate || undefined,
        recordDate: recordDate || undefined,
        notes: notes || undefined,
      });
      continue;
    }

    if (isNaN(quantity) || quantity <= 0 || isNaN(price) || price <= 0) {
      errors.push(`Row ${i + 1}: invalid data (${symbol})`);
      continue;
    }

    rows.push({
      symbol,
      quantity,
      price,
      amount: quantity * price,
      date: date,
      notes: notes || undefined,
    });
  }

  return { rows, errors };
}

export function sampleCsv(kind: CsvImportKind): string {
  if (kind === 'sell') {
    return 'Symbol,Quantity,SellPrice,SoldDate\nTCS,10,4200,2026-03-15\n';
  }
  if (kind === 'dividend') {
    return 'Symbol,Quantity,Amount,DividendDate,ExDate,RecordDate,Notes\nTCS,10,120,2026-06-18,2026-06-10,2026-06-12,Interim\n';
  }
  return 'Symbol,Quantity,PurchasePrice,PurchaseDate\nTCS,10,3500,2025-01-10\n';
}

export function downloadTextFile(filename: string, content: string): void {
  const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(url);
}
