import { PortfolioHolding } from './portfolio-holding.model';

export interface GroupedHolding {
  stockId: number;
  symbol: string;
  companyName: string;
  logoUrl?: string | null;
  exchange: number;
  lotCount: number;
  remainingQuantity: number;
  remainingInvestment: number;
  avgBuyPrice: number;
  currentPrice: number;
  currentValue: number;
  unrealizedGainLoss: number;
  unrealizedGainLossPercent: number;
  oldestPurchaseDate: string;
  lots: PortfolioHolding[];
}

export interface FifoPreviewRow {
  holdId: number;
  purchaseDate: string;
  purchasePrice: number;
  remainingQuantity: number;
  sellQuantity: number;
  costAmount: number;
}