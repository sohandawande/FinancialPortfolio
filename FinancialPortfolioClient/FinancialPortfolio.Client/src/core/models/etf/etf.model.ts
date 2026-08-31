export interface Etf {
  id: number;
  etfId: number;
  symbol: string;
  companyName: string;
  industry: string;
  isinCode: string;
  series: string;
  category: string;
  currentPrice: number;
  previousClose: number;
  openPrice: number;
  highPrice: number;
  lowPrice: number;
  volume: number;
  averageVolume: number;
  week52High: number;
  week52Low: number;
  pe: number;
  eps: number;
  marketCap: number;
  priceChange: number;
  isActive: boolean;
  logoUrl?: string | null;
  lastUpdated: string;
  createdDate?: string;
  modifiedDate?: string;
  /** ETF-specific optional fields */
  trackingIndex?: string | null;
  expenseRatio?: number | null;
  aum?: number | null;
}
