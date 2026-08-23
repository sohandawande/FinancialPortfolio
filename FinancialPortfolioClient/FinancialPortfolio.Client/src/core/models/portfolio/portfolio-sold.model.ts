export interface PortfolioSold {
  id: number;
  holdId: number;
  stockId: number;
  symbol: string;
  companyName: string;
  logoUrl?: string | null;
  exchange: number;
  sellQuantity: number;
  sellPrice: number;
  sellAmount: number;
  purchasePrice: number;
  costAmount: number;
  realizedGainLoss: number;
  realizedGainLossPercent: number;
  holdDays?: number | null;
  lotStatus: number;
  purchaseDate: string;
  soldDate: string;
  soldNotes?: string | null;
}