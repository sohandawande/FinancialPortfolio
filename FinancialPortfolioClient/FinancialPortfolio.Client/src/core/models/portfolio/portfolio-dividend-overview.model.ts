import { PortfolioDividend } from './portfolio-dividend.model';
import { PortfolioDividendStockGroup } from './portfolio-dividend-stock-group.model';
import { PortfolioDividendYearGroup } from './portfolio-dividend-year-group.model';

export interface PortfolioDividendOverview {
  totalAmount: number;
  companyCount: number;
  payoutCount: number;
  stocks: PortfolioDividendStockGroup[];
  years: PortfolioDividendYearGroup[];
  payouts: PortfolioDividend[];
}

export type DividendListView = 'stocks' | 'payouts' | 'year';
