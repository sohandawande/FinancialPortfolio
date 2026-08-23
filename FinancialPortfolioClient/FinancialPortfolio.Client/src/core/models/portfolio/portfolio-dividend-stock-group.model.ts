import { PortfolioDividend } from './portfolio-dividend.model';

export interface PortfolioDividendStockGroup {
  stockId: number;
  symbol: string;
  companyName: string;
  logoUrl?: string | null;
  exchange: number;
  totalAmount: number;
  payoutCount: number;
  totalShares: number;
  lastDividendDate?: string | null;
  payouts: PortfolioDividend[];
}
