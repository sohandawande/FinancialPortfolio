import { PortfolioDividend } from './portfolio-dividend.model';

export interface PortfolioDividendYearGroup {
  year: number;
  amount: number;
  payoutCount: number;
  companyCount: number;
  payouts: PortfolioDividend[];
}
