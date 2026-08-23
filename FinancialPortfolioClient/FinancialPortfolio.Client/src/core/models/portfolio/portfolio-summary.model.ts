import { PortfolioDividendYearTotal } from './portfolio-dividend-year-total.model';

export interface PortfolioSummary {
  portfolioId: number;
  name: string;
  totalInvestment: number;
  totalCurrentValue: number;
  unrealizedGainLoss: number;
  unrealizedGainLossPercent: number;
  realizedProfitBooked: number;
  totalDividendsReceived: number;
  totalBrokerageTax: number;
  totalHoldLots: number;
  totalSoldLots: number;
  totalStocksHold: number;
  totalStocksSold: number;
  totalStocksHoldSell: number;
  lastUpdated?: string;
  dividendsByYear?: PortfolioDividendYearTotal[];
}
