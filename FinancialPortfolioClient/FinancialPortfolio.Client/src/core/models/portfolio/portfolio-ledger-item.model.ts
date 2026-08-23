export interface PortfolioLedgerItem {
  serialNo: number;
  id: number;
  holdId?: number | null;
  soldId?: number | null;
  stockId: number;
  companyName: string;
  symbol: string;
  stockCode: string;
  logoUrl?: string | null;
  exchange: number;
  netQuantity: number;
  purchasePrice: number;
  marketPrice: number;
  totalInvestment: number;
  totalCurrentValue: number;
  totalGainLoss: number;
  gainLossPercent: number;
  holdDays: number;
  currentType: number;
  asOfDate: string;
  purchaseDate: string;
  exitDate?: string | null;
  sellPrice?: number | null;
  totalOnSell?: number | null;
  profitLoss: number;
}