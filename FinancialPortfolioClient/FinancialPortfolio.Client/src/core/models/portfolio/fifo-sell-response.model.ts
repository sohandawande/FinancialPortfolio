import { PortfolioSold } from './portfolio-sold.model';

export interface FifoSellResponse {
  stockId: number;
  symbol: string;
  companyName: string;
  totalSellQuantity: number;
  sellPrice: number;
  totalSellAmount: number;
  totalCostAmount: number;
  totalRealizedGainLoss: number;
  totalRealizedGainLossPercent: number;
  lotsConsumed: number;
  allocations: PortfolioSold[];
}