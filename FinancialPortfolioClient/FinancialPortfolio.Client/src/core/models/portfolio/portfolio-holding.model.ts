export interface PortfolioHolding {
  id: number;
  portfolioId: number;
  stockId: number;
  symbol: string;
  companyName: string;
  logoUrl?: string | null;
  exchange: number;
  quantity: number;
  remainingQuantity: number;
  purchasePrice: number;
  investmentAmount: number;
  remainingInvestment: number;
  currentPrice: number;
  currentValue: number;
  unrealizedGainLoss: number;
  unrealizedGainLossPercent: number;
  holdDays?: number | null;
  lotStatus: number;          // 1=Open, 2=PartialSold, 3=FullySold
  isSold: boolean;
  purchaseDate: string;
  exitDate?: string | null;
  holdNotes?: string | null;
  createdDate?: string;
  modifiedDate?: string;
}